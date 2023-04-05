// -----------------------------------------------------------------------------
// PROJECT   : CathodeRay
// COPYRIGHT : Andy Thomas (C) 2023
// LICENSE   : LGPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/CathodeRay
//
// This file is part of CathodeRay.
//
// CathodeRay is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
// Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// CathodeRay is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for
// more details.
//
// You should have received a copy of the GNU Lesser General Public License along with CathodeRay.
// If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

using System.Text;
using KuiperZone.CathodeRay.Internal;
using KuiperZone.CathodeRay.Utils;

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// Provides a file and directory browser page. The style and behaviour is specified by the
    /// <see cref="BrowserStyles"/> flags.
    /// </summary>
    public class FileBrowserPage : CathodeRayPage
    {
        private const int ItemSpacer = 2;
        private const int MinFileLength = 17;
        private const int MaxTabFileLength = 35;
        private const int TimeWidth = 16 + 1;
        private const int NominalMaxItemWidth = 70;

        private const string FilePrefix = "";
        private const string DirPrefix = "D";
        private const string TimeFormat = "yyyy-MM-dd HH:mm";
        private const string WaitMessage = "Please wait ... ";

        // Keep letters in one place so we can see any conflicts
        private const string SelectDirOpt = "T";
        private const string RenameDirOpt = "R";
        private const string NewFileOpt = "N";
        private const string DeleteDirOpt = "X";
        private const string DeleteMultiFilesOpt = "A";
        private const string GotoDrivesOpt = "G";
        private const string DetailedViewOpt = "V";
        private const string ReverseSortOpt = "S";
        private const string QuitBrowserOpt = "Q";

        // I.e. "1000 bytes" + 2
        private static readonly int SizeWidth = 12;

        // Alias
        private static readonly char PathSep = Path.DirectorySeparatorChar;

        private bool _subChild;
        private bool _reversedSorted;
        private bool _detailedView;
        private readonly FileBrowserPage? _rootBrowser;

        private bool _directoryExists;
        private DirectoryInfo? _directory;

        private int _maxItemWidth;
        private readonly List<string?> _files = new();
        private readonly List<string?> _subdirs = new();
        private readonly List<DriveQuery> _driveQueries = new List<DriveQuery>();

        /// <summary>
        /// Constructor. A parent page and <see cref="BrowserStyles"/> value should be specified.
        /// If title is null, a default value will be chosen by on the supplied style.
        /// </summary>
        public FileBrowserPage(CathodeRayPage parent, BrowserStyles style, string? title = null)
            : base(parent, ChooseTitle(title, style))
        {
            Styling = style;

            _reversedSorted = style.HasFlag(BrowserStyles.Reversed);
            _detailedView = style.HasFlag(BrowserStyles.DetailedView);

            // Initialise
            DirectoryPath = "";
        }

        /// <summary>
        /// Private copy constructor. The <see cref="DirectoryPath"/> is not set.
        /// </summary>
        private FileBrowserPage(FileBrowserPage parent)
            : this(parent, parent.Styling, parent.PageTitle)
        {
            _rootBrowser = parent._rootBrowser ?? parent;

            _reversedSorted = parent._reversedSorted;
            _detailedView = parent._detailedView;

            FileFilter = parent.FileFilter;
            HighlightAge = parent.HighlightAge;
            CommandMenuHeader = parent.CommandMenuHeader;
            BrowserMenuHeader = parent.BrowserMenuHeader;
        }

        /// <summary>
        /// The <see cref="BrowserStyles"/> flags passed to the constructor.
        /// </summary>
        public BrowserStyles Styling { get; }

        /// <summary>
        /// A header string for directory and file specific commands. Setting a null or empty value
        /// will disable the display of this string, but not the options themselves.
        /// </summary>
        public string CommandMenuHeader { get; set; } = "Commands";

        /// <summary>
        /// A header string for common browser options. Setting a null or empty value will
        /// disable the display of this string, but not the options themselves.
        /// </summary>
        public string BrowserMenuHeader { get; set; } = "Browse";

        /// <summary>
        /// Gets or sets the directory path. The path string does not terminate with a path
        /// separator. Setting an empty string will assign the current working directory. If
        /// null is set, the resulting path will depend on whether the <see cref="Styling"/> has the
        /// <see cref="BrowserStyles.RootNavigation"/> flag. If it does, <see cref="DirectoryPath"/>
        /// will be set to null which will allow the user to navigate the drives the drives on the
        /// local machine. Note, therefore <see cref="DirectoryPath"/> can be null. If the flag is
        /// not set, <see cref="DirectoryPath"/> will be set to the user's home directory. The
        /// initial value is current working directory.
        /// </summary>
        public string? DirectoryPath
        {
            get { return _directory?.FullName; }

            set
            {
                if (value == null)
                {
                    if (Styling.HasFlag(BrowserStyles.RootNavigation))
                    {
                        _directory = null;
                    }
                    else
                    {
                        _directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    }
                }
                else
                if (string.IsNullOrWhiteSpace(value))
                {
                    _directory = new DirectoryInfo(Environment.CurrentDirectory);
                }
                else
                {
                    _directory = new DirectoryInfo(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a file wild-card pattern which acts to filter the file items displayed.
        /// This parameter can contain a combination of valid literal path and wildcard (* and ?)
        /// characters, but it doesn't support regular expressions. For example, the string "*.txt"
        /// would cause only text files to be shown. Setting a null or empty string applies no
        /// filter and all file items are shown. The default is null.
        /// </summary>
        public string? FileFilter { get; set; }

        /// <summary>
        /// Gets or sets a TimeSpan which allows files or directories which have been recently
        /// modified to be highlighted. Items with modification times more recent than this span in
        /// the past will be shown in a highlighted color. Ignored if the value is TimeSpan.Zero
        /// (default).
        /// </summary>
        public TimeSpan HighlightAge { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets the path return result when used with the <see cref="BrowserStyles.SelectItemToReturn"/>
        /// flag option. If the user selects a file or directory, the value will provide the fully
        /// qualified path when <see cref="CathodeRayPage.Execute"/> returns. If the user does not make
        /// a selection, the value will be null.
        /// </summary>
        public string? SelectedItem { get; private set; }

        /// <summary>
        /// Gets or sets whether  user selected "Quits browser". This value is static.
        /// </summary>
        protected static bool GlobalExitBrowser { get; set; }

        /// <summary>
        /// Gets whether navigation to the parent directory is possible.
        /// </summary>
        protected bool AllowNavigationToParent
        {
            get
            {
                if (_subChild)
                {
                    return true;
                }

                if (_directory != null && Styling.HasFlag(BrowserStyles.RootNavigation))
                {
                    return Styling.HasFlag(BrowserStyles.FixedDriveNavigation) || _directory.Parent != null;
                }

                return false;
            }
        }

        /// <summary>
        /// True if the browser is showing common options.
        /// </summary>
        protected bool HasViewOptions
        {
            get { return Styling.HasFlag(BrowserStyles.ShowViewOptions) && _directory != null; }
        }

        /// <summary>
        /// True if "Goto drives" is visible.
        /// </summary>
        protected bool CanGotoDrives
        {
            get { return Styling.HasFlag(BrowserStyles.FixedDriveNavigation) && _directory != null; }
        }

        /// <summary>
        /// True if the directory is selectable.
        /// </summary>
        protected bool CanSelectDirectory
        {
            get
            {
                return Styling.HasFlag(BrowserStyles.SelectItemToReturn)
                    && Styling.HasFlag(BrowserStyles.ShowDirectories)
                    && !Styling.HasFlag(BrowserStyles.ShowFiles)
                    && _directory != null && _directoryExists;
            }
        }

        /// <summary>
        /// True if the directory can be renamed or deleted.
        /// </summary>
        protected bool CanRenameDeleteDirectory
        {
            get
            {
                return Styling.HasFlag(BrowserStyles.RenameDeleteDirectory)
                    && _directory != null && _directoryExists && _directory.Parent != null;
            }
        }

        /// <summary>
        /// True if a new file can be selected.
        /// </summary>
        protected bool CanSelectNewFile
        {
            get
            {
                return Styling.HasFlag(BrowserStyles.ShowFiles)
                    && Styling.HasFlag(BrowserStyles.SelectItemToReturn)
                    && Styling.HasFlag(BrowserStyles.AllowSelectNewFile)
                    && _directory != null && _directoryExists;
            }
        }

        /// <summary>
        /// True if user can delete multiple files.
        /// </summary>
        protected bool CanDeleteMultiFiles
        {
            get
            {
                return Styling.HasFlag(BrowserStyles.ShowFiles)
                    && Styling.HasFlag(BrowserStyles.DeleteMultiFiles)
                    && _directory != null && _directoryExists && _files.Count > 0;
            }
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnExecutionStarted"/>.
        /// </summary>
        protected override void OnExecutionStarted()
        {
            SelectedItem = null;
            GlobalExitBrowser = false;

            base.OnExecutionStarted();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.PrintMain"/>.
        /// </summary>
        protected override void PrintMain()
        {
            ScreenIO.PrintLn();

            try
            {
                if (Styling.HasFlag(BrowserStyles.ShowDirectoryName))
                {
                    if (DirectoryPath != null)
                    {
                        ScreenIO.Print("Directory: ");
                    }

                    ScreenIO.PrintLn(GetDirectoryAsTitle(), ColorId.Title);
                    ScreenIO.PrintLn();

                }

                try
                {
                    // May take several seconds to fire up dormant drives
                    var pos0 = ScreenIO.PosXY;
                    ScreenIO.Print(WaitMessage);

                    RebuildContent();

                    // Erase WaitMessage
                    ScreenIO.PosXY = pos0;
                    ScreenIO.Print(new string(' ', WaitMessage.Length));
                    ScreenIO.PosXY = pos0;
                }
                catch
                {
                    ScreenIO.PrintLn();
                    throw;
                }
                finally
                {
                    RebuildMenu();
                }

                if (_directoryExists)
                {
                    if (!PrintDrives(_driveQueries))
                    {
                        // Provide a hint as to desired max width
                        int maxItem = Math.Max(_maxItemWidth, NominalMaxItemWidth);

                        if (Styling.HasFlag(BrowserStyles.ShowDirectories))
                        {
                            PrintItems(_subdirs, maxItem, true);
                        }

                        if (Styling.HasFlag(BrowserStyles.ShowFiles))
                        {
                            if (Styling.HasFlag(BrowserStyles.ShowDirectories))
                            {
                                ScreenIO.PrintLn();
                            }

                            PrintItems(_files, maxItem, false);
                        }
                    }
                }
                else
                {
                    ScreenIO.PrintLn("DIRECTORY NOT EXIST", ColorId.Warning);
                }

            }
            catch (Exception e)
            {
                ScreenIO.PrintException(e, false);
            }

            base.PrintMain();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.PrintTitle"/>.
        /// </summary>
        protected override void PrintTitle()
        {
            if (Styling.HasFlag(BrowserStyles.ShowDirectoryAsPageTitle))
            {
                ScreenIO.PrintLn(GetDirectoryAsTitle(), ColorId.Title, TitleFormat);
                ScreenIO.PrintLn();
            }
            else
            if (_rootBrowser != null)
            {
                _rootBrowser.PrintTitle();
            }
            else
            {
                base.PrintTitle();
            }
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.LogicHandler"/>.
        /// </summary>
        protected override PageLogic LogicHandler(string? userInput)
        {
            // DIRECTORY NAVIGATION
            if (Styling.HasFlag(BrowserStyles.SubNavigation))
            {
                int idx = ParseItem(DirPrefix, _subdirs, userInput);

                if (idx >= 0)
                {
                    var child = new FileBrowserPage(this);
                    bool toParentDir = _subdirs[idx]?.Equals("..") == true;

                    if (toParentDir)
                    {
                        if (_subChild)
                        {
                            // Fall back to parent page
                            return PageLogic.ExitToParent;
                        }

                        // Set null if parent is null
                        child.DirectoryPath = _directory?.Parent?.FullName;
                    }
                    else
                    if (DirectoryPath == null)
                    {
                        // Root directory
                        child._subChild = true;
                        child.DirectoryPath = _subdirs[idx];
                    }
                    else
                    {
                        // Sub-directory
                        child._subChild = true;
                        child.DirectoryPath = DirectoryPath + PathSep + _subdirs[idx];
                    }

                    return ExecuteChild(child);
                }
            }

            // FILES
            if (Styling.HasFlag(BrowserStyles.ShowFiles))
            {
                int idx = ParseItem(FilePrefix, _files, userInput);

                if (idx >= 0)
                {
                    // Using root browser as parent prevents undesirable title chain
                    var parent = _rootBrowser ?? this;

                    // Full file path
                    var path = DirectoryPath + PathSep + _files[idx];

                    if (Styling.HasFlag(BrowserStyles.SelectItemToReturn))
                    {
                        if (ConfirmUserSelection(_files[idx]))
                        {
                            SelectedItem = path;
                            GlobalExitBrowser = true;
                            return PageLogic.ExitToParent;
                        }

                        // Not necessary to reprint
                        ScreenIO.PrintLn();
                        return PageLogic.Reprompt;
                    }

                    if (Styling.HasFlag(BrowserStyles.DirectToFile))
                    {
                        if (Styling.HasFlag(BrowserStyles.FileViewContent))
                        {
                            new FileContentPage(parent, _files[idx]).Execute(path);
                            return PageLogic.Reprint;
                        }

                        if (Styling.HasFlag(BrowserStyles.FileOpenRun))
                        {
                            ScreenIO.Print("Result: ");

                            if (FileUtils.OpenRun(path, true))
                            {
                                ScreenIO.PrintLn("Opened OK", ColorId.Success);
                            }
                            else
                            {
                                ScreenIO.PrintLn("Failed", ColorId.Critical);
                            }

                            ScreenIO.PrintLn();
                            return PageLogic.Reprompt;
                        }
                    }

                    new FileSummaryPage(parent).Execute(path);
                    return PageLogic.Reprint;
                }
            }

            return base.LogicHandler(userInput);
        }

        private static string? ChooseTitle(string? title, BrowserStyles style)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                if (style.HasFlag(BrowserStyles.ShowDirectories)
                    && !style.HasFlag(BrowserStyles.ShowFiles))
                {
                    return "Directory Browser";
                }

                return "File Browser";
            }

            return title;
        }

        private static int ParseItem(string prefix, IList<string?> items, string? input)
        {
            if (input != null)
            {
                input = input.Trim();

                if (input.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (int.TryParse(input.Substring(prefix.Length), out int idx)
                        && idx > 0 && idx <= items.Count)
                    {
                        return idx - 1;
                    }
                }

                // Cannot search for file name itself as may conflict
                // with other options, but can make exception for ".."
                if (input.Equals(".."))
                {
                    // Should be first for directories
                    return items.IndexOf("..");
                }
            }

            return -1;
        }

        private void RebuildMenu()
        {
            Menu.Clear();
            bool spacer = false;

            // Directory commands
            if (CanSelectNewFile || CanSelectDirectory || CanDeleteMultiFiles || CanRenameDeleteDirectory)
            {
                if (!string.IsNullOrEmpty(CommandMenuHeader))
                {
                    spacer = true;
                    Menu.Add(new MenuItem(CommandMenuHeader));
                }

                if (CanSelectNewFile)
                {
                    Menu.Add(new MenuItem(NewFileOpt, "New File", NewFileHandler));
                }

                if (CanSelectDirectory)
                {
                    Menu.Add(new MenuItem(SelectDirOpt, "Select this Directory", SelectDirHandler));
                }

                if (CanDeleteMultiFiles)
                {
                    Menu.Add(new MenuItem(DeleteMultiFilesOpt, "Delete Files", DeleteMultiFilesHandler));
                }

                if (CanRenameDeleteDirectory)
                {
                    Menu.Add(new MenuItem(RenameDirOpt, "Rename Directory", RenameDirHandler));
                    Menu.Add(new MenuItem(DeleteDirOpt, "Delete Directory", DeleteDirHandler));
                }
            }

            // Browser options
            if (CanGotoDrives || HasViewOptions || Styling.HasFlag(BrowserStyles.ShowQuitOption))
            {
                if (!string.IsNullOrEmpty(BrowserMenuHeader))
                {
                    if (spacer) Menu.Add(null);
                    Menu.Add(new MenuItem(BrowserMenuHeader));
                    spacer = true;
                }

                if (CanGotoDrives)
                {
                    Menu.Add(new MenuItem(GotoDrivesOpt, "Goto Drives", GotoDrivesHandler));
                }

                if (HasViewOptions)
                {
                    Menu.Add(new MenuItem(ReverseSortOpt, "Reverse Sort", _reversedSorted, ReverseSortHandler));
                    Menu.Add(new MenuItem(DetailedViewOpt, "Detailed View", _detailedView, DetailedViewHandler));
                }

                if (Styling.HasFlag(BrowserStyles.ShowQuitOption))
                {
                    Menu.Add(new MenuItem(QuitBrowserOpt, "Quit " + PageTitle, QuitBrowserHandler));
                }
            }
        }

        private PageLogic SelectDirHandler(MenuItem _)
        {
            if (ConfirmUserSelection(_directory?.Name))
            {
                GlobalExitBrowser = true;
                SelectedItem = DirectoryPath;
                return PageLogic.ExitToParent;
            }

            ScreenIO.PrintLn();
            return PageLogic.Reprompt;
        }

        private PageLogic RenameDirHandler(MenuItem _)
        {
            var prompt = new Prompter(PromptStyle.FileName);

            // Limit on name part only
            prompt.MaxLength = 255;
            prompt.Prefix = "New name?: ";

            if (prompt.Execute() == PromptStatus.Entered)
            {
                var dest = _directory?.Parent?.FullName;

                if (!string.IsNullOrEmpty(dest))
                {
                    dest += PathSep + prompt.InputString;

                    ScreenIO.PrintLn();
                    ScreenIO.Print("Old path: ", ColorId.Gray);
                    ScreenIO.PrintLn(DirectoryPath);

                    ScreenIO.Print("New path: ", ColorId.Gray);
                    ScreenIO.PrintLn(dest);

                    ScreenIO.PrintLn();

                    prompt = new Prompter(PromptStyle.Confirm);
                    prompt.Prefix = "Rename this directory? [%Y%/%N%]: ";

                    if (prompt.Execute() == PromptStatus.Yes && DirectoryPath != null)
                    {
                        ScreenIO.Print("Result: ");
                        Directory.Move(DirectoryPath, dest);

                        DirectoryPath = dest;
                        ScreenIO.PrintLn("Renamed OK", ColorId.Success);
                    }
                }
                else
                {
                    // Not expected
                    ScreenIO.PrintLn("Failed", ColorId.Critical);
                }

                ScreenIO.PrintLn();
                new Prompter(PromptStyle.AnyKey).Execute();
            }

            return PageLogic.Reprint;
        }

        private PageLogic NewFileHandler(MenuItem _)
        {
            var prompt = new Prompter(PromptStyle.FileName);

            if (prompt.Execute() == PromptStatus.Entered)
            {
                GlobalExitBrowser = true;
                SelectedItem = DirectoryPath + PathSep + prompt.InputString;
                return PageLogic.ExitToParent;
            }

            return PageLogic.Reprint;
        }

        private PageLogic DeleteDirHandler(MenuItem _)
        {
            ScreenIO.PrintLn();
            ScreenIO.Print("Directory: ", ColorId.Gray);
            ScreenIO.PrintLn(DirectoryPath);

            if (!IsDirectoryEmpty(DirectoryPath))
            {
                ScreenIO.PrintLn("Files and subdirectories will be removed!", ColorId.Warning);
            }

            ScreenIO.PrintLn();

            var prompt = new Prompter(PromptStyle.Confirm);
            prompt.Prefix = "Delete this directory? [%Y%/%N%]: ";

            if (prompt.Execute() == PromptStatus.Yes && DirectoryPath != null)
            {
                ScreenIO.Print("Result: ");
                ScreenIO.PrintLn(DirectoryPath);

                Directory.Delete(DirectoryPath, true);

                ScreenIO.PrintLn("Deleted OK", ColorId.Success);

                ScreenIO.PrintLn();
                new Prompter(PromptStyle.AnyKey).Execute();

                return PageLogic.ExitToParent;
            }

            return PageLogic.Reprint;
        }

        private PageLogic DeleteMultiFilesHandler(MenuItem _)
        {
            var prompt = new Prompter();
            prompt.Prefix = "Wildcard: ";
            prompt.MinLength = 1;

            if (prompt.Execute() != PromptStatus.Entered)
            {
                return PageLogic.Reprint;
            }

            var list = new List<string?>();
            var wildcard = new WildcardMatcher(prompt.InputString, true);

            foreach (var fname in _files)
            {
                if (wildcard.IsMatch(fname))
                {
                    list.Add(fname);
                }
            }

            if (list.Count == 0)
            {
                throw new ArgumentException("No files match");
            }

            prompt = new Prompter(PromptStyle.Confirm);
            prompt.Prefix = $"Delete {list.Count} files? [%Y%/%N%]: ";

            if (prompt.Execute() == PromptStatus.Yes)
            {
                ScreenIO.PrintLn();
                ScreenIO.PrintLn("Deleting...");

                int deleted = 0;
                int count = list.Count;

                foreach (var fname in list)
                {
                    try
                    {
                        File.Delete(DirectoryPath + PathSep + fname);
                        deleted += 1;
                    }
                    catch (IOException e)
                    {
                        ScreenIO.Print("Failed: ", ColorId.Critical);
                        ScreenIO.PrintLn(fname);
                        ScreenIO.PrintLn(e.Message);
                        ScreenIO.PrintLn();
                    }
                }

                ScreenIO.Print("Result: ");
                ScreenIO.PrintLn(deleted + " files deleted out of " + count, deleted > 0 ? ColorId.Success : ColorId.Warning);

                ScreenIO.PrintLn();
                new Prompter(PromptStyle.AnyKey).Execute();
            }

            return PageLogic.Reprint;
        }

        private PageLogic GotoDrivesHandler(MenuItem _)
        {
            var child = new FileBrowserPage(this)
            {
                DirectoryPath = null
            };
            return ExecuteChild(child);
        }

        private PageLogic DetailedViewHandler(MenuItem _)
        {
            _detailedView = !_detailedView;
            return PageLogic.Reprint;
        }

        private PageLogic ReverseSortHandler(MenuItem _)
        {
            _reversedSorted = !_reversedSorted;
            return PageLogic.Reprint;
        }

        private PageLogic QuitBrowserHandler(MenuItem _)
        {
            GlobalExitBrowser = true;
            return PageLogic.ExitToParent;
        }

        private PageLogic ExecuteChild(FileBrowserPage child)
        {
            child.Execute();

            if (GlobalExitBrowser)
            {
                SelectedItem = child.SelectedItem;
                return PageLogic.ExitToParent;
            }

            return PageLogic.Reprint;
        }

        private bool ConfirmUserSelection(string? name)
        {
            if (Styling.HasFlag(BrowserStyles.ConfirmBeforeSelectReturn))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    ScreenIO.Print("Selected: ");
                    ScreenIO.PrintLn(name, ColorId.Gray);
                    ScreenIO.PrintLn();
                }

                var prompt = new Prompter(PromptStyle.Confirm);
                return prompt.Execute() == PromptStatus.Yes;
            }

            return true;
        }

        private string GetDirectoryAsTitle()
        {
            if (_directory == null)
            {
                return "LOCAL MACHINE";
            }

            if (Styling.HasFlag(BrowserStyles.RootNavigation))
            {
                return _directory.FullName;
            }

            var page = Parent;
            string name = _directory.Name;

            while (page is FileBrowserPage browser && browser.DirectoryPath != null)
            {
                page = page.Parent;
                name = new DirectoryInfo(browser.DirectoryPath).Name + PathSep + name;
            }

            return name;
        }

        private DateTime GetModTime(string? name, bool subdir)
        {
            if (_directory != null && name != null)
            {
                try
                {
                    var path = _directory.FullName + PathSep + name;

                    if (subdir)
                    {
                        return Directory.GetLastWriteTimeUtc(path);
                    }

                    return File.GetLastWriteTimeUtc(path);
                }
                catch
                {
                }
            }

            return new DateTime();
        }

        private string GetModString(string? name, bool subdir)
        {
            var dt = GetModTime(name, subdir);

            if (dt.Ticks > 0)
            {
                if (Styling.HasFlag(BrowserStyles.UtcTimes))
                {
                    return dt.ToString(TimeFormat + "Z");
                }

                return dt.ToLocalTime().ToString(TimeFormat);
            }

            return "?";
        }

        private string GetFileSize(string? name)
        {
            if (_directory != null && name != null)
            {
                try
                {
                    var fi = new FileInfo(_directory.FullName + PathSep + name);
                    return BitByte.ToByteString(fi.Length);
                }
                catch
                {
                }
            }

            return "?";
        }

        private int CalcColumnWidth(int maxWidth, out int width)
        {
            // Detailed width
            int printWidth = ScreenIO.ActualWidth;
            width = printWidth - TimeWidth - SizeWidth - 2 * ItemSpacer;

            if (_detailedView && width >= MinFileLength)
            {
                if (maxWidth > 0)
                {
                    width = Math.Min(maxWidth, width);
                }

                // Detailed view
                return 0;
            }

            // Tabbed
            int temp = Math.Clamp(printWidth / 3, MinFileLength, MaxTabFileLength);
            width = temp - ItemSpacer;

            // Number of columns
            return printWidth / temp;
        }

        private void PrintItems(IList<string?> items, int maxItemWidth, bool isSubdirs)
        {
            string? header = null;
            int columnCount = CalcColumnWidth(maxItemWidth, out int itemWidth);

            if (Styling.HasFlag(BrowserStyles.ShowSectionHeaders))
            {
                // Block header
                header = isSubdirs ? "Subdirectories:" : "Files:";

                string? mod = null;
                string? size = null;

                if (columnCount == 0)
                {
                    // Detailed
                    header = header.PadRight(itemWidth + ItemSpacer);
                    mod = "Modified".PadRight(TimeWidth + ItemSpacer);
                    size = !isSubdirs ? "Size" : "";

                    size = size.PadRight(SizeWidth);
                }

                ScreenIO.PrintLn(header + mod + size, ColorId.Gray);
            }

            if (items.Count == 0)
            {
                if (header != null)
                {
                    ScreenIO.PrintLn();
                }

                ScreenIO.PrintLn("[None]");
                return;
            }

            string pfx = isSubdirs ? DirPrefix : FilePrefix;

            int padNum = pfx.Length + 3;
            if (items.Count >= 100) padNum += 2;
            else if (items.Count >= 10) padNum += 1;

            long highTick = 0;

            if (HighlightAge > TimeSpan.Zero)
            {
                highTick = DateTime.UtcNow.Subtract(HighlightAge).Ticks;
            }

            for (int n = 0; n < items.Count; ++n)
            {
                // Item name and numbered prefix
                string? str = items[n];

                if (!isSubdirs && Styling.HasFlag(BrowserStyles.HideExtension))
                {
                    str = Path.GetFileNameWithoutExtension(str);
                }

                if (!isSubdirs || Styling.HasFlag(BrowserStyles.SubNavigation))
                {
                    // Numbers start at 1
                    str = (pfx + (n + 1).ToString() + ". ").PadRight(padNum) + str;
                }

                // Determine item color based on file age
                var color = ColorId.Text;

                if (highTick > 0 && items[n] != ".."
                    && GetModTime(items[n], isSubdirs).Ticks > highTick)
                {
                    color = ColorId.High;
                }

                if (columnCount == 0)
                {
                    // Detailed view
                    str = ScreenIO.Truncate(str, itemWidth, Truncation.EllipsesCenter)?.PadRight(itemWidth + ItemSpacer);

                    // Mod time and size
                    str += GetModString(items[n], isSubdirs).PadRight(TimeWidth + ItemSpacer);

                    string size = "";
                    if (!isSubdirs) size = GetFileSize(items[n]);

                    ScreenIO.PrintLn(str + size.PadRight(SizeWidth), color);
                }
                else
                {
                    // Column view
                    if (n >= columnCount && n % columnCount == 0)
                    {
                        ScreenIO.PrintLn();
                    }

                    ScreenIO.Print(ScreenIO.Truncate(str, itemWidth, Truncation.EllipsesCenter)?.PadRight(itemWidth + ItemSpacer), color);

                    if (n == items.Count - 1)
                    {
                        ScreenIO.PrintLn();
                    }
                }
            }
        }

        private static string GetDriveSize(DriveQuery drive)
        {
            if (drive.IsValid && drive.TotalFreeSpace >= 0 && drive.TotalSize > 0)
            {
                var sb = new StringBuilder();

                sb.Append(BitByte.ToByteString(drive.TotalSize - drive.TotalFreeSpace));
                sb.Append(" of ");
                sb.Append(BitByte.ToByteString(drive.TotalSize));

                return sb.ToString();
            }

            return "";
        }

        private static bool IsDirectoryEmpty(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var info = new DirectoryInfo(path);

                    foreach (var sd in info.EnumerateDirectories())
                    {
                        if (sd != null) return false;
                    }

                    foreach (var fi in info.EnumerateFiles())
                    {
                        if (fi != null) return false;
                    }

                    return true;
                }
                catch (DirectoryNotFoundException)
                {
                    return true;
                }
                catch (IOException)
                {
                }
            }

            // Assume false
            return false;
        }

        private bool PrintDrives(IList<DriveQuery> items)
        {
            // Returns true if items printed
            if (_driveQueries.Count != 0)
            {
                const int Spc = ItemSpacer;
                string? typName = null;

                int padNum = DirPrefix.Length + 3;
                if (items.Count >= 100) padNum += 2;
                else if (items.Count >= 10) padNum += 1;

                int printWidth = ScreenIO.ActualWidth;
                int volWidth = Math.Clamp(25 * printWidth / 100, 10 + Spc, 35) - Spc;
                int fmtWidth = Math.Clamp(13 * printWidth / 100, 6 + Spc, 12) - Spc;
                int sizeWidth = Math.Clamp(32 * printWidth / 100, SizeWidth + Spc, SizeWidth * 2 + 4);

                // Space available for path
                int pathWidth = printWidth - volWidth - fmtWidth - sizeWidth - 3 * Spc;

                // Examine first drive
                var drv = _driveQueries[0];

                if (string.Equals(drv.VolumeLabel, drv.RootDirectory?.FullName))
                {
                    // No distinct path
                    pathWidth = 0;
                    volWidth = printWidth - fmtWidth - sizeWidth - 2 * Spc;
                }

                // Cut it down if too large
                pathWidth = Math.Min(pathWidth, Math.Max(_maxItemWidth, NominalMaxItemWidth));

                for (int n = 0; n < items.Count; ++n)
                {
                    drv = items[n];
                    var color = ColorId.Gray;

                    // Headers
                    if (!string.Equals(typName, drv.DriveType.ToString()))
                    {
                        typName = drv.DriveType.ToString();

                        // Type name aligns with volume
                        var header = ScreenIO.Truncate(typName + ":", volWidth, Truncation.EllipsesEnd)?.PadRight(volWidth + Spc);

                        if (n == 0)
                        {
                            if (pathWidth != 0)
                            {
                                header += "Path".PadRight(pathWidth + Spc);
                            }

                            header += "Format".PadRight(fmtWidth + Spc);
                            header += "Size".PadRight(sizeWidth);
                        }
                        else
                        {
                            // Line spacer
                            ScreenIO.PrintLn();

                            int cnt = pathWidth != 0 ? 2 : 1;
                            header += "".PadRight(pathWidth + fmtWidth + sizeWidth + cnt * Spc);
                        }

                        ScreenIO.PrintLn(header, color);
                    }

                    var pfx = new string(' ', padNum + 1);
                    var path = drv.RootDirectory?.FullName;

                    if (drv.IsValid && !string.IsNullOrEmpty(path))
                    {
                        color = ColorId.Text;
                        pfx = (DirPrefix + (n + 1).ToString() + ". ").PadRight(padNum);
                    }

                    var line = ScreenIO.Truncate(pfx + drv.VolumeLabel, volWidth, Truncation.EllipsesCenter)?.PadRight(volWidth + Spc);
                    if (pathWidth != 0) line += ScreenIO.Truncate(path, pathWidth, Truncation.EllipsesEnd)?.PadRight(pathWidth + Spc);

                    line += ScreenIO.Truncate(drv.DriveFormat, fmtWidth, Truncation.EllipsesEnd)?.PadRight(fmtWidth + Spc);
                    line += ScreenIO.Truncate(GetDriveSize(drv), sizeWidth, Truncation.EllipsesEnd)?.PadRight(sizeWidth);

                    ScreenIO.PrintLn(line, color);
                }

                return true;
            }

            return false;
        }

        private void RebuildContent()
        {
            _files.Clear();
            _subdirs.Clear();
            _driveQueries.Clear();
            _directoryExists = false;
            _maxItemWidth = 0;

            if (_directory == null)
            {
                // Drives
                if (Styling.HasFlag(BrowserStyles.FixedDriveNavigation))
                {
                    _driveQueries.AddRange(DriveQuery.GetDrives(DriveType.Fixed));
                }

                if (Styling.HasFlag(BrowserStyles.NetworkDriveNavigation))
                {
                    _driveQueries.AddRange(DriveQuery.GetDrives(DriveType.Network));
                }

                if (Styling.HasFlag(BrowserStyles.GlobalDriveNavigation))
                {
                    _driveQueries.AddRange(DriveQuery.GetDrives(DriveType.Removable));
                    _driveQueries.AddRange(DriveQuery.GetDrives(DriveType.CDRom));
                }

                foreach (var drv in _driveQueries)
                {
                    var root = drv.RootDirectory?.FullName;

                    _subdirs.Add(root);

                    if (root != null && root.Length > _maxItemWidth)
                    {
                        _maxItemWidth = root.Length;
                    }

                }

                // Set last on success
                _directoryExists = true;
            }
            else
            if (_directory.Exists && Directory.Exists(_directory.FullName))
            {
                // Directories
                if (Styling.HasFlag(BrowserStyles.ShowDirectories))
                {
                    foreach (var di in _directory.EnumerateDirectories())
                    {
                        _subdirs.Add(di.Name);

                        if (di.Name.Length > _maxItemWidth)
                        {
                            _maxItemWidth = di.Name.Length;
                        }
                    }

                    _subdirs.Sort(StringComparer.InvariantCultureIgnoreCase);

                    if (_reversedSorted)
                    {
                        _subdirs.Reverse();
                    }

                    if (AllowNavigationToParent)
                    {
                        _subdirs.Insert(0, "..");
                    }
                }

                if (Styling.HasFlag(BrowserStyles.ShowFiles))
                {
                    // Files
                    bool showHidden = Styling.HasFlag(BrowserStyles.ShowAllFiles);
                    var enmtr = _directory.EnumerateFiles(string.IsNullOrWhiteSpace(FileFilter) ? "*" : FileFilter.Trim());

                    foreach (var fi in enmtr)
                    {
                        var at = fi.Attributes;

                        if (showHidden || (!at.HasFlag(FileAttributes.Hidden) && !at.HasFlag(FileAttributes.System)))
                        {
                            _files.Add(fi.Name);

                            if (fi.Name.Length > _maxItemWidth)
                            {
                                _maxItemWidth = fi.Name.Length;
                            }

                        }
                    }

                    _files.Sort(StringComparer.InvariantCultureIgnoreCase);

                    if (_reversedSorted)
                    {
                        _files.Reverse();
                    }
                }

                // Set last on success
                _directoryExists = true;
            }
        }

    }
}
