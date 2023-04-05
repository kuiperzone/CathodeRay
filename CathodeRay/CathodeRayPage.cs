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

namespace KuiperZone.CathodeRay
{
    /// <summary>
    /// Provides a base class intended to facilitate the construction of a terminal based user
    /// interface comprising a hierarchy of "pages". <see cref="CathodeRayPage"/>, and its related classes
    /// provide a scalable framework in which each screen within the console application interface
    /// is implemented as a separate class type derived from this base class. Custom application
    /// subclasses are required to override and extend designated methods in order to print content,
    /// add functionality and implement navigation from one page to another. Although this class is
    /// concrete, it will display little, except a prompt, unless subclasses override and implement
    /// the relevant protected methods in order provide the content to display and respond to user input.
    /// </summary>
    public abstract class CathodeRayPage
    {
        private static bool s_globalExitToRoot;
        private static bool s_globalExitProgram;

        private CathodeRayPage[]? _chain;
        private bool _isExecuting;

        /// <summary>
        /// Default constructor. The instance will be a root page with no parent.
        /// </summary>
        public CathodeRayPage()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructor. The instance will be a root page with no parent.
        /// The PageTitle is set to the supplied value.
        /// </summary>
        public CathodeRayPage(string? title)
            : this(null, title)
        {
        }

        /// <summary>
        /// Child constructor. The instance will be a child of the given parent with the PageTitle
        /// supplied. If parent is null, the new instance will be a root page. Note that child pages
        /// inherit key properties of the root instance, including the TitleSeparator and TitleFormat.
        /// </summary>
        public CathodeRayPage(CathodeRayPage? parent, string? title)
        {
            PageTitle = title ?? GetType().Name;

            if (parent != null)
            {
                // Inherited properties
                Parent = parent;
                PageRoot = Parent.PageRoot;
                PageDepth = Parent.PageDepth + 1;

                ExecutionStarted = PageRoot.ExecutionStarted;
                PrintStarted = PageRoot.PrintStarted;
                PrintFinished = PageRoot.PrintFinished;
                ExecutionFinished = PageRoot.ExecutionFinished;

                TitleSeparator = PageRoot.TitleSeparator;
                TitleFormat = PageRoot.TitleFormat;
                NavigationHeader = PageRoot.NavigationHeader;
                PromptMessage = PageRoot.PromptMessage;
                UnknownMessage = PageRoot.UnknownMessage;
            }
            else
            {
                PageRoot = this;
            }
        }

        /// <summary>
        /// Raised when <see cref="Execute"/> is called. Unlike <see cref="PrintStarted"/>, this
        /// event can be raised only once per execution. The value of this property is inherited
        /// from <see cref="PageRoot"/> during construction and there is no need to add the same
        /// event for child pages.
        /// </summary>
        public event EventHandler? ExecutionStarted;

        /// <summary>
        /// Raised when the page is about to printed. This is an appropriate place to print common
        /// header text. Unlike <see cref="ExecutionStarted"/>, this event can be raised more than
        /// once per execution. The value of this property is inherited from <see cref="PageRoot"/> during
        /// construction and there is no need to add the same event for child pages.
        /// </summary>
        public event EventHandler? PrintStarted;

        /// <summary>
        /// Raised when the page has finished printing, but before the page waits on the input
        /// prompt. Unlike <see cref="ExecutionFinished"/>, this event can be raised more than once
        /// per execution. The value of this property is inherited from <see cref="PageRoot"/> during
        /// construction and there is no need to add the same event for child pages.
        /// </summary>
        public event EventHandler? PrintFinished;

        /// <summary>
        /// Raised when <see cref="Execute"/> has finished and is about to return. Unlike <see
        /// cref="PrintFinished"/>, this event can be raised only once per execution. The value of
        /// this property is inherited from <see cref="PageRoot"/> during construction and there is
        /// no need to add the same event for child pages.
        /// </summary>
        public event EventHandler? ExecutionFinished;

        /// <summary>
        /// Page display option flags.
        /// </summary>
        public static PageOptions GlobalOptions { get; set; } = PageOptions.Default;

        /// <summary>
        /// Gets or sets the CLS navigation option strings. Navigation options are always printed, even if
        /// <see cref="Menu"/> is empty. Tuple Item1 is the input shortcut, whereas Item2 is the text.
        /// Assigning an empty string to either item disables it.
        /// </summary>
        public static Tuple<string, string> ClsMenuOption { get; set; } = Tuple.Create("CLS", "Clear Screen");

        /// <summary>
        /// Gets or sets the BACK navigation option strings. Navigation options are always printed, even if
        /// <see cref="Menu"/> is empty.Tuple Item1 is the input shortcut, whereas Item2 is the text. Note for this
        /// option, the <see cref="CathodeRayPage.Execute"/> method maps the Escape key to the Item1 string.
        /// Assigning an empty string to either item disables it.
        /// </summary>
        public static Tuple<string, string> BackMenuOption { get; set; } = Tuple.Create("ESC", "Previous Page");

        /// <summary>
        /// Gets or sets the HOME navigation option strings. Navigation options are always printed, even if
        /// <see cref="Menu"/> is empty. Tuple Item1 is the input shortcut, whereas Item2 is the text.
        /// Assigning an empty string to either item disables it.
        /// </summary>
        public static Tuple<string, string> HomeMenuOption { get; set; } = Tuple.Create("HOME", "Home Page");

        /// <summary>
        /// Gets or sets the EXIT navigation option strings. Navigation options are always printed, even if
        /// <see cref="Menu"/> is empty. Tuple Item1 is the input shortcut, whereas Item2 is the text.
        /// Assigning an empty string to either item disables it.
        /// </summary>
        public static Tuple<string, string> ExitMenuOption { get; set; } = Tuple.Create("EXIT", "Exit Program");

        /// <summary>
        /// Gets or sets a string value which can be shown as a prefix to indicate that an option
        /// or flag is active. The value of this property is inherited from <see cref="PageRoot"/>
        /// during construction. Setting it to null or empty means that it will not be shown. See
        /// also: <see cref="MenuItem.IsSuffixed"/>.
        /// </summary>
        public static string? FlagSuffix { get; set; } = " [*]";

        /// <summary>
        /// Gets the menu collection instance. The value is not inherited from the root page. If the
        /// menu items are to be static, i.e. never change, <see cref="Menu"/> may be populated in
        /// the subclass constructor. If the menu items are dynamic, i.e. may change on each
        /// display, population should be performed in <see cref="OnPrintStarted"/> which should
        /// clear the collection before repopulating it. Adding a null item is allowed and serves as
        /// a vertical space.
        /// </summary>
        public ICollection<MenuItem?> Menu { get; } = new List<MenuItem?>();

        /// <summary>
        /// Gets the root page instance. If <see cref="PageRoot"/> is called on the root page, it
        /// returns itself.
        /// </summary>
        public CathodeRayPage PageRoot { get; }

        /// <summary>
        /// Gets the page depth order. If <see cref="PageDepth"/> is 0, the page instance is the
        /// root page. The immediate children of the root page will have <see cref="PageDepth"/> of 1.
        /// </summary>
        public int PageDepth { get; }

        /// <summary>
        /// Gets the Parent the page before this in the user interface page hierarchy. Specifically,
        /// the parent page is responsible for creating the child page instance and calling its
        /// <see cref="Execute"/> method. If Parent is null, this page instance is the root page.
        /// </summary>
        public CathodeRayPage? Parent { get; }

        /// <summary>
        /// Gets the page title. If the instance was constructed without a title, the class name
        /// is initially assigned instead. However, assigning a null or empty string to this property
        /// will show no page title. See also <see cref="QualifiedTitle"/>.
        /// </summary>
        public string? PageTitle { get; set; }

        /// <summary>
        /// Gets the qualified page title. This is the concatenation of the <see cref="PageTitle"/>
        /// values in the root-to-child sequence, much like a qualified directory path. If <see
        /// cref="TitleSeparator"/> is set to null, however, <see cref="QualifiedTitle"/> simply
        /// returns the child's <see cref="PageTitle"/>.
        /// </summary>
        public string? QualifiedTitle
        {
            get { return MakeQualifiedTitle(false); }
        }

        /// <summary>
        /// Gets or sets short string value used as a separator between <see cref="PageTitle"/>
        /// elements in the <see cref="QualifiedTitle"/>. The value of this property is inherited
        /// from <see cref="PageRoot"/> during construction. Note that assigning null to <see
        /// cref="TitleSeparator"/> will cause <see cref="QualifiedTitle"/> to give only the <see
        /// cref="PageTitle"/> value.
        /// </summary>
        public string? TitleSeparator { get; set; } = " > ";

        /// <summary>
        /// Gets or sets the print format to be used by the <see cref="PrintTitle"/> method. The
        /// initial value of this property is inherited from <see cref="PageRoot"/> during construction.
        /// </summary>
        public ScreenOptions TitleFormat { get; set; } = ScreenOptions.WordWrap;

        /// <summary>
        /// Page navigation header string. Setting a null or empty value will disable this. The
        /// value of this property is inherited from the <see cref="PageRoot"/> during construction.
        /// </summary>
        public string? NavigationHeader { get; set; } = "Page Navigation";

        /// <summary>
        /// Gets or set a short message to display when prompting the user for input.
        /// The value of this property is inherited from the <see cref="PageRoot"/> during construction.
        /// </summary>
        public string? PromptMessage { get; set; } = "Ready: ";

        /// <summary>
        /// Gets or sets a short message to be printed when <see cref="LogicHandler"/> returns
        /// the <see cref="PageLogic.Unknown"/> result. It is typically used to indicate that
        /// the input was invalid (i.e. "Invalid option"). If <see cref="UnknownMessage"/> is
        /// null, nothing is printed. The value of this property is inherited from the
        /// <see cref="PageRoot"/> during construction.
        /// </summary>
        public string? UnknownMessage { get; set; } = "Invalid option";

        /// <summary>
        /// Gets the input response from the user. Specifically, it is the result returned by <see
        /// cref="InputPrompt"/>. Until <see cref="InputPrompt"/> is called, <see cref="UserInput"/>
        /// is null.
        /// </summary>
        public string? UserInput { get; private set; }

        /// <summary>
        /// Gets whether <see cref="Execute"/> has been called. It is initially false but is set to
        /// true just before <see cref="Execute"/> returns. <see cref="HasExecuted"/> can be
        /// reset by assignment.
        /// </summary>
        public bool HasExecuted { get; set; }

        /// <summary>
        /// Gets the chain of pages leading to this one, where index 0 is <see cref="PageRoot"/>
        /// and last item page on which <see cref="GetChain"/> is called. If page on which it is
        /// called is the root page, the result will be an array of size 1.
        /// </summary>
        public CathodeRayPage[] GetChain()
        {
            // Lazy value
            if (_chain == null)
            {
                var temp = new List<CathodeRayPage>
                {
                    this
                };

                var page = Parent;

                while (page != null)
                {
                    temp.Insert(0, page);
                    page = page.Parent;
                }

                _chain = temp.ToArray();
            }

            return _chain;
        }

        /// <summary>
        /// Performs the execution loop, printing the page contents and waiting on the user input
        /// prompt. The input given by the user will typically determine whether this method should
        /// return, or whether the sequence should be repeated as part of an execution loop.
        /// Specifically, <see cref="Execute"/> calls the following sequence of methods: 1a. <see
        /// cref="OnExecutionStarted"/>, 2a. <see cref="OnPrintStarted"/>, 2b. <see
        /// cref="PrintTitle"/>, 2c. <see cref="PrintMain"/>, 2d. <see cref="OnPrintFinished"/>, 2e.
        /// <see cref="InputPrompt"/>, 2f. <see cref="LogicHandler"/> and 3a. <see
        /// cref="OnExecutionFinished"/>. Routines 1 and 3 are called only once at the start and end
        /// of execution respectively, whereas those designated 2 are called in a loop which breaks
        /// according to the return value of <see cref="InputPrompt"/>.
        /// </summary>
        public void Execute()
        {
            if (_isExecuting)
            {
                // Recursion on the same instance - can't see this being a good thing
                throw new InvalidOperationException(
                    "Cannot call Execute() on a page which is currently executing (recursion prohibited)");
            }

            // Initialize
            _isExecuting = true;
            s_globalExitToRoot = false;
            s_globalExitProgram = false;

            bool cursorVisible = ScreenIO.IsCursorVisible;
            ScreenIO.IsCursorVisible = false;

            // Start sequence
            OnExecutionStarted();

            try
            {
                PageLogic logic = PageLogic.Reprint;

                do
                {
                    ScreenIO.IsCursorVisible = false;
                    var navMenu = CreateNavigationMenu();

                    if (logic == PageLogic.Reprint)
                    {
                        if (GlobalOptions.HasFlag(PageOptions.AutoCls))
                        {
                            ScreenIO.Cls();
                        }
                        else
                        if (ScreenIO.PosY > 0)
                        {
                            ScreenIO.Reset();
                            ScreenIO.PrintLn();
                        }

                        OnPrintStarted();

                        try
                        {
                            PrintTitle();
                            PrintMain();
                        }
                        catch (Exception e)
                        {
                            ScreenIO.Reset();
                            ScreenIO.PrintException(e, true);
                        }

                        ScreenIO.Reset();

                        PrintMenu(navMenu);
                        OnPrintFinished();
                    }

                    UserInput = InputPrompt();

                    // Handle navigation first and separately
                    logic = LogicHandlerInternal(navMenu, UserInput);

                    if (logic == PageLogic.Unknown)
                    {
                        logic = LogicHandler(UserInput);

                        if (logic == PageLogic.Unknown)
                        {
                            ScreenIO.PrintLn(UnknownMessage, ColorId.Warning);
                        }
                    }

                } while (RepeatLogic(logic));

            }
            finally
            {
                OnExecutionFinished();

                _isExecuting = false;
                HasExecuted = true;
                ScreenIO.IsCursorVisible = cursorVisible;

                if (Parent == null)
                {
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Returns true if the two strings are considered equals. The comparison is case
        /// insensitive and is trimmed left and right. This method is intended for use in
        /// <see cref="LogicHandler(string)"/> override handlers and, while its use is optional, it
        /// provides a consistent and convenient approach to comparing user input strings.
        /// </summary>
        protected static bool InputEquals(string? expect, string? input)
        {
            if (expect != null && input != null)
            {
                return string.Equals(expect, input.Trim(), StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// The base implementation raises the <see cref="ExecutionStarted"/> event. It may be
        /// overridden to perform initialization on each execution. If overridden, the base
        /// implementation should be called. It is not expected that this routine will print output.
        /// </summary>
        protected virtual void OnExecutionStarted()
        {
            OnEvent(ExecutionStarted);
        }

        /// <summary>
        /// The base implementation raises the <see cref="PrintStarted"/> event. If overridden, the
        /// base implementation should be called.
        /// </summary>
        protected virtual void OnPrintStarted()
        {
            OnEvent(PrintStarted);
        }

        /// <summary>
        /// Prints the page title. This method is called after <see cref="OnPrintStarted"/>,
        /// but before <see cref="PrintMain"/>, and renders the <see cref="PageTitle"/>.
        /// It does nothing unless the <see cref="PageOptions.ShowTitle"/> is enabled. This
        /// method may be overridden to display a custom title format.
        /// </summary>
        protected virtual void PrintTitle()
        {
            if (GlobalOptions.HasFlag(PageOptions.ShowTitle))
            {
                string? title = PageTitle;

                if (GlobalOptions.HasFlag(PageOptions.ShowQualifiedTitle))
                {
                    title = MakeQualifiedTitle(GlobalOptions.HasFlag(PageOptions.SkipRootTitle));

                    if (GlobalOptions.HasFlag(PageOptions.RevertPageTitleIfQualifiedIsLong)
                        && title != null && title.Length > ScreenIO.ActualWidth)
                    {
                        title = PageTitle;
                    }
                }

                if (!string.IsNullOrEmpty(title))
                {
                    ScreenIO.PrintLn(title, ColorId.Title, TitleFormat);
                }
            }
        }

        /// <summary>
        /// This method prints the "main content" of the page and the base implementation prints the
        /// <see cref="Menu"/> text. It is called by <see cref="Execute"/> after <see
        /// cref="PrintTitle"/>, but before the page navigation menus are printed. It can be
        /// overridden to print custom content, but the overriding implementation should always call
        /// the base method to print <see cref="Menu"/>. There is no event associated with this
        /// method. If the implementation throws an exception, the exception message and stack will
        /// be printed to Console.
        /// </summary>
        protected virtual void PrintMain()
        {
            PrintMenu(Menu);
        }

        /// <summary>
        /// The base implementation raises the <see cref="PrintFinished"/> event. If overridden, the
        /// base implementation should be called.
        /// </summary>
        protected virtual void OnPrintFinished()
        {
            OnEvent(PrintFinished);
        }

        /// <summary>
        /// The base implementation raises the <see cref="ExecutionFinished"/> event. If overridden,
        /// the base implementation should be called.
        /// </summary>
        protected virtual void OnExecutionFinished()
        {
            OnEvent(ExecutionFinished);
        }

        /// <summary>
        /// Displays the page prompt and waits for input. It is called after the print sequence and,
        /// thus, the prompt is shown at the bottom of the page. It can be overridden for custom
        /// behaviour and could potentially prompt the user for multiple inputs. However, it must
        /// return a string value which represents the primary user response. The return value will
        /// be assigned to <see cref="UserInput"/> and passed as the argument to <see
        /// cref="LogicHandler(string)"/>. If overriding, note that this method is expected to
        /// block, returning only when the user presses a key or hits "enter" (failing to do this
        /// may cause <see cref="Execute"/> may loop indefinitely). Note that the result may be null
        /// if the user presses the Escape key.
        /// </summary>
        protected virtual string? InputPrompt()
        {
            // Convention is that sections start with new line.
            ScreenIO.PrintLn();

            var prompt = new Prompter()
            {
                Prefix = PromptMessage,
                ShortCuts = true,
            };

            // Map ESC to back page
            if (prompt.Execute() == PromptStatus.Escaped)
            {
                if (IsEnabled(BackMenuOption))
                {
                    return BackMenuOption?.Item1;
                }

                return null;
            }

            return prompt.InputString;
        }

        /// <summary>
        /// Calls the first <see cref="MenuItem.Handler"/> found in the <see cref="Menu"/>
        /// collection with a <see cref="MenuItem.Shortcut"/> string which matches "userInput",
        /// returning the handler result. If no matching <see cref="MenuItem"/> is found, the return
        /// value is <see cref="PageLogic.Unknown"/>. The <see cref="InputEquals"/> method is used
        /// to perform the shortcut comparison. It can be overridden for custom behaviour.
        /// </summary>
        protected virtual PageLogic LogicHandler(string? userInput)
        {
            return LogicHandlerInternal(Menu, userInput);
        }

        private static bool IsEnabled(Tuple<string, string> item)
        {
            return item.Item1.Length != 0 && item.Item2.Length != 0;
        }

        private void PrintMenu(ICollection<MenuItem?> menu)
        {
            if (menu.Count != 0)
            {
                ScreenIO.PrintLn();
            }

            foreach (var item in menu)
            {
                if (item != null)
                {
                    item.PrintLn();
                }
                else
                {
                    // Vertical spacer
                    ScreenIO.PrintLn();
                }
            }
        }

        private PageLogic LogicHandlerInternal(ICollection<MenuItem?> menu, string? userInput)
        {
            if (userInput != null)
            {
                try
                {
                    // Enter re-prints
                    if (string.IsNullOrWhiteSpace(userInput))
                    {
                        return PageLogic.Reprint;
                    }

                    foreach (var item in menu)
                    {
                        if (item != null && item.Handler != null && InputEquals(item.Shortcut, userInput))
                        {
                            return item.Handler(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    ScreenIO.PrintException(e, false);
                    ScreenIO.PrintLn();
                    new Prompter(PromptStyle.AnyKey).Execute();
                    return PageLogic.Reprint;
                }
            }

            return PageLogic.Unknown;
        }

        private ICollection<MenuItem?> CreateNavigationMenu()
        {
            const int Pad = 6;
            var menu = new List<MenuItem?>();

            if (!string.IsNullOrEmpty(NavigationHeader))
            {
                menu.Add(new MenuItem(NavigationHeader));
            }

            if (!GlobalOptions.HasFlag(PageOptions.AutoCls) && IsEnabled(ClsMenuOption))
            {
                menu.Add(new MenuItem(ClsMenuOption.Item1, ClsMenuOption.Item2, ClsHandler, null, Pad));
            }

            if (PageDepth > 0 && IsEnabled(BackMenuOption))
            {
                menu.Add(new MenuItem(BackMenuOption.Item1, BackMenuOption.Item2, BackHandler, null, Pad));
            }

            if (PageDepth > 0 && IsEnabled(HomeMenuOption))
            {
                menu.Add(new MenuItem(HomeMenuOption.Item1, HomeMenuOption.Item2, HomeHandler, null, Pad));
            }

            if (IsEnabled(ExitMenuOption))
            {
                menu.Add(new MenuItem(ExitMenuOption.Item1, ExitMenuOption.Item2, ExitHandler, null, Pad));
            }

            return menu;
        }

        private PageLogic ClsHandler(MenuItem _)
        {
            ScreenIO.Cls();
            return PageLogic.Reprint;
        }

        private PageLogic BackHandler(MenuItem _)
        {
            return PageLogic.ExitToParent;
        }

        private PageLogic HomeHandler(MenuItem _)
        {
            return PageLogic.ExitToRoot;
        }

        private PageLogic ExitHandler(MenuItem _)
        {
            return PageLogic.ExitAll;
        }

        private void OnEvent(EventHandler? handler)
        {
            if (handler != null)
            {
                try
                {
                    handler.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    ScreenIO.PrintException(e, true);
                }
            }
        }

        private string? MakeQualifiedTitle(bool skipRoot)
        {
            // Build the title string passed to PrintTitle().
            var result = PageTitle;
            var sep = TitleSeparator;

            if (Parent != null && sep != null)
            {
                // Build sequence
                result = Parent.MakeQualifiedTitle(skipRoot);

                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(PageTitle))
                {
                    if (skipRoot && Parent.Parent == null)
                    {
                        // Remove root title
                        result = null;
                    }
                    else
                    {
                        result += sep;
                    }


                    result += PageTitle;
                }
            }

            return result;
        }

        private bool RepeatLogic(PageLogic value)
        {
            // Given the result of PageLogic(), returns true if the execution loop should repeat.
            if (!s_globalExitProgram && (!s_globalExitToRoot || PageDepth == 0))
            {
                if (value == PageLogic.Reprint || value == PageLogic.Unknown
                    || value == PageLogic.Reprompt)
                {
                    // Reprint OK
                    return true;
                }

                // Force execution unwinding.
                s_globalExitProgram = value == PageLogic.ExitAll;
                s_globalExitToRoot = value == PageLogic.ExitToRoot;
            }

            // Execution finished
            return false;
        }

    }
}
