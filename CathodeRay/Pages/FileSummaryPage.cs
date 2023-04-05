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

using System.Globalization;
using System.Text;
using KuiperZone.CathodeRay.Internal;
using KuiperZone.CathodeRay.Utils;

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// Show file summary information and optional content view.
    /// </summary>
    public class FileSummaryPage : CathodeRayPage
    {
        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string OpenOpt = "O";
        private const string RenameOpt = "R";
        private const string DeleteOpt = "X";
        private const string ViewTextOpt = "T";
        private const string ViewBinaryOpt = "B";

        private FileInfo? _fileInfo;
        private Encoding? _encoding;

        /// <summary>
        /// Constructor overload which maps its initial property values from the given
        /// <see cref="BrowserStyles"/>. The parent cannot be null.
        /// </summary>
        public FileSummaryPage(FileBrowserPage parent, string? title = "File Information")
            : base(parent, title)
        {
            var style = parent.Styling;
            UtcTimes = style.HasFlag(BrowserStyles.UtcTimes);
            AllowRenameDelete = style.HasFlag(BrowserStyles.RenameDeleteFiles);
            AllowViewContent = style.HasFlag(BrowserStyles.FileViewContent);
            AllowOpenRun = style.HasFlag(BrowserStyles.FileOpenRun);
        }

        /// <summary>
        /// The full filename path. See <see cref="Execute(string)"/>.
        /// </summary>
        public string? FilePath { get; private set; }

        /// <summary>
        /// Show dates and times as UTC.
        /// </summary>
        public bool UtcTimes { get; set; }

        /// <summary>
        /// Allow the user to view the file content within the page (assumes text).
        /// </summary>
        public bool AllowViewContent { get; set; }

        /// <summary>
        /// Allow the user to rename and delete the file.
        /// </summary>
        public bool AllowRenameDelete { get; set; }

        /// <summary>
        /// Allow the user to open the file on the local machine.
        /// </summary>
        public bool AllowOpenRun { get; set; }

        /// <summary>
        /// A header string for file specific options. Setting a null or empty value will
        /// disable this.
        /// </summary>
        public string? MenuHeader { get; set; } = "Commands";

        /// <summary>
        /// Overload for <see cref="CathodeRayPage.Execute"/> with supplied filename. The
        /// <see cref="FilePath"/> property will be set to file path on return. The filename can be
        /// null, in which case it is displayed in the console as not existing.
        /// </summary>
        public void Execute(string filename)
        {
            FilePath = null;

            if (!string.IsNullOrWhiteSpace(filename))
            {
                FilePath = new FileInfo(filename).FullName;
            }

            Execute();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnPrintStarted"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            Menu.Clear();

            _encoding = null;

            if (FilePath != null)
            {
                _fileInfo = new FileInfo(FilePath);

                if (_fileInfo.Exists)
                {
                    _encoding = _fileInfo.Length > 0 ? FileUtils.GetEncoding(FilePath) : null;

                    if (AllowRenameDelete || AllowViewContent || AllowOpenRun)
                    {
                        if (!string.IsNullOrEmpty(MenuHeader))
                        {
                            Menu.Add(new MenuItem(MenuHeader));
                        }

                        if (AllowOpenRun)
                        {
                            Menu.Add(new MenuItem(OpenOpt, "Open File", OpenHandler));
                        }

                        if (AllowRenameDelete)
                        {
                            Menu.Add(new MenuItem(RenameOpt, "Rename File", RenameHandler));
                            Menu.Add(new MenuItem(DeleteOpt, "Delete File", DeleteHandler));
                        }

                        if (AllowViewContent)
                        {
                            if (_encoding != null)
                            {
                                Menu.Add(new MenuItem(ViewTextOpt, "View Text", ViewTextHandler));
                            }

                            Menu.Add(new MenuItem(ViewBinaryOpt, "View Binary", ViewBinaryHandler));
                        }
                    }
                }
            }

            base.OnPrintStarted();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.PrintMain"/>.
        /// </summary>
        protected override void PrintMain()
        {
            const int Pad = 12;

            ScreenIO.PrintLn();
            ScreenIO.Print("Filename: ".PadRight(Pad));
            ScreenIO.PrintLn(Path.GetFileName(FilePath), ColorId.Title);
            ScreenIO.PrintLn();

            if (_fileInfo?.Exists == true)
            {
                ScreenIO.PrintLn("Size:".PadRight(Pad) + BitByte.ToByteString(_fileInfo.Length, true));
                ScreenIO.Print("Encoding:".PadRight(Pad));

                if (_fileInfo.Length > 0)
                {
                    ScreenIO.PrintLn(FileUtils.GetEncodingName(_encoding));
                }
                else
                {
                    _encoding = null;
                    ScreenIO.PrintLn("Empty");
                }

                ScreenIO.PrintLn();
                ScreenIO.PrintLn("Created:".PadRight(Pad) + Timestamp(_fileInfo.CreationTime));
                ScreenIO.PrintLn("Modified:".PadRight(Pad) + Timestamp(_fileInfo.LastWriteTime));

                string? attribs = null;
                string sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

                foreach (var a in Enum.GetValues(typeof(FileAttributes)))
                {
                    if (_fileInfo.Attributes.HasFlag((FileAttributes)a))
                    {
                        if (attribs != null)
                        {
                            attribs += sep + " ";
                        }

                        attribs += a.ToString();
                    }
                }

                ScreenIO.PrintLn("Attributes:".PadRight(Pad) + attribs);
            }
            else
            {
                ScreenIO.PrintLn("NOT EXIST", ColorId.Warning);
            }

            base.PrintMain();
        }

        private PageLogic RenameHandler(MenuItem _)
        {
            var prompt = new Prompter(PromptStyle.FileName);

            // Limit on name part only
            prompt.MaxLength = 255;
            prompt.Prefix = "New filename?: ";

            if (prompt.Execute() == PromptStatus.Entered)
            {
                ScreenIO.Print("Result: ");
                var dest = Path.GetDirectoryName(FilePath ?? throw new ArgumentNullException(nameof(FilePath)));

                if (!string.IsNullOrEmpty(dest))
                {
                    dest += Path.DirectorySeparatorChar + prompt.InputString;

                    File.Move(FilePath, dest);

                    FilePath = dest;
                    ScreenIO.PrintLn("Renamed OK", ColorId.Success);
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

        private PageLogic DeleteHandler(MenuItem _)
        {
            var prompt = new Prompter(PromptStyle.Confirm);
            prompt.Prefix = "Delete this file? [%Y%/%N%]: ";

            if (prompt.Execute() == PromptStatus.Yes)
            {
                ScreenIO.Print("Result: ");
                File.Delete(FilePath ?? throw new ArgumentNullException(nameof(FilePath)));

                ScreenIO.PrintLn("Deleted OK", ColorId.Success);

                ScreenIO.PrintLn();
                new Prompter(PromptStyle.AnyKey).Execute();

                return PageLogic.ExitToParent;
            }

            return PageLogic.Reprint;
        }

        private PageLogic OpenHandler(MenuItem _)
        {
            ScreenIO.Print("Result: ");

            if (FileUtils.OpenRun(FilePath ?? throw new ArgumentNullException(nameof(FilePath)), true))
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

        private PageLogic ViewTextHandler(MenuItem _)
        {
            var page = new FileContentPage(this, Path.GetFileName(FilePath ?? throw new ArgumentNullException(nameof(FilePath))))
            {
                SourceEncoding = _encoding ?? throw new ArgumentNullException(nameof(_encoding))
            };
            page.Execute(FilePath);
            return PageLogic.Reprint;
        }

        private PageLogic ViewBinaryHandler(MenuItem _)
        {
            var page = new FileContentPage(this, Path.GetFileName(FilePath ?? throw new ArgumentNullException(nameof(FilePath))))
            {
                BinaryContent = true
            };
            page.Execute(FilePath);
            return PageLogic.Reprint;
        }

        private string Timestamp(DateTime dt)
        {
            if (UtcTimes)
            {
                return dt.ToUniversalTime().ToString(TimeFormat + "Z");
            }

            return dt.ToString(TimeFormat);
        }
    }
}
