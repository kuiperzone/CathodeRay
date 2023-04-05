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

using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// A subclass of <see cref="CathodeRayPage"/> which allows the user to view and edit primitive property
    /// values of an instance of class type T. These include: integers, double, decimal and enum types. Complex
    /// property values are ignored and not shown. By default, only properties with public getters and setters are
    /// shown. The generic class may use the Description attribute on properties and this will be shown as help
    /// information. I.e. [Description("OAM port number. Default is 38500.")]
    /// </summary>
    public class PrimitivePropertiesPage<T> : CathodeRayPage
        where T : class
    {
        private readonly bool _isHelpScreen;

        /// <summary>
        /// Help prefix.
        /// </summary>
        protected const string HelpPrefix = "Help: ";

        /// <summary>
        /// String show for null property values.
        /// </summary>
        public const string NullString = "{null}";

        /// <summary>
        /// String show for empty strings.
        /// </summary>
        public const string EmptyString = "{empty}";

        /// <summary>
        /// Constructor with instance of data. The caller should make a clone as changes will be made
        /// on prior to the user confirming those changes.
        /// </summary>
        public PrimitivePropertiesPage(CathodeRayPage? parent, T props, string? title = "Properties")
            : base(parent, title)
        {
            Properties = props ?? throw new ArgumentNullException(nameof(props));

            if (parent is PrimitivePropertiesPage<T> propPage)
            {
                _isHelpScreen = true;
                ShowReadonlyProperties = propPage.ShowReadonlyProperties;
                Inclusions = propPage.Inclusions;
                Exclusions = propPage.Exclusions;
            }
        }

        /// <summary>
        /// Gets the generic type instance supplied on construction. A protected setter is provided. The value should not be set to null.
        /// </summary>
        public T Properties { get; protected set; }

        /// <summary>
        /// Gets whether the user has set any value of <see cref="Properties"/> during
        /// the <see cref="CathodeRayPage.Execute"/> method. The initial value is false.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Gets whether the user specifically selected "Accept" in order to leave the page. If <see cref="ShowConfirmChanges"/>
        /// is false, this value will always be false.
        /// </summary>
        public bool IsConfirmed { get; protected set; }

        /// <summary>
        /// Gets or set the properties sub-header. If null or empty, no sub-header is shown.
        /// </summary>
        public string SubHeader { get; set; } = "Properties";

        /// <summary>
        /// Gets or sets the Accept shortcut.
        /// </summary>
        public string ConfirmShortcut { get; set; } = "Y";

        /// <summary>
        /// Gets or sets the Accept text.
        /// </summary>
        public string ComfirmChangesText { get; set; } = "Confirm";

        /// <summary>
        /// Gets or sets whether the class allows modification of <see cref="Properties"/>.
        /// If true, the user can view but not change values.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether to show readonly properties.
        /// </summary>
        public bool ShowReadonlyProperties { get; set; }

        /// <summary>
        /// Gets or sets whether an empty string input value converts to null when set.
        /// </summary>
        public bool EmptyToNull { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show valid enum inputs on prompt.
        /// </summary>
        public bool ShowEnumsOnPrompt { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show help information, if available. This make use of "Description" attribute.
        /// </summary>
        public bool ShowHelp { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show an "Accept changes" option. See also <see cref="IsConfirmed"/>.
        /// </summary>
        public bool ShowConfirmChanges { get; set; }

        /// <summary>
        /// Gets or sets whether <see cref="IsModified"/> is always true. This is applicable where
        /// the properties instance is new and allows the user to accept them without modification.
        /// </summary>
        public bool IsAlwaysModified { get; set; }

        /// <summary>
        /// Gets or sets whether to confirm that changes should be abandoned.
        /// </summary>
        public bool ConfirmAbandon { get; set; }

        /// <summary>
        /// Gets a list of property names to explicitly include. If the list is not
        /// empty, only those list will be shown.
        /// </summary>
        public IList<string> Inclusions { get; } = new List<string>();

        /// <summary>
        /// Gets a list of property names to explicitly exclude.
        /// </summary>
        public IList<string> Exclusions { get; } = new List<string>();

        /// <summary>
        /// Gets a list of additional properties items. These typically refer to complex types are
        /// appended after the properties list prior to "help" and "accept" options.
        /// </summary>
        protected IList<MenuItem?> ComplexProperties { get; } = new List<MenuItem?>();

        /// <summary>
        /// Called when the user selects the accept option. A return value of true allows execution to return to
        /// the page parent, whereas false will re-print the page. This method may be overridden to write data to storage.
        /// The default implementation returns true but otherwise does nothing.
        /// </summary>
        protected virtual bool OnConfirmed()
        {
            return true;
        }

        /// <summary>
        /// Overrides <see cref="CathodeRayPage.OnExecutionStarted"/>.
        /// </summary>
        protected override void OnExecutionStarted()
        {
            IsConfirmed = false;
            IsModified = IsAlwaysModified;
            base.OnExecutionStarted();
        }

        /// <summary>
        /// Overrides <see cref="CathodeRayPage.OnPrintStarted"/>. Clears and recreates <see cref="CathodeRayPage.Menu"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            Menu.Clear();

            if (!_isHelpScreen)
            {
                bool hasHelp = false;

                if (!string.IsNullOrEmpty(SubHeader))
                {
                    Menu.Add(new MenuItem(SubHeader));
                }

                int n = 1;
                var props = Properties.GetType().GetProperties();
                Array.Sort(props, new NameComparer());

                foreach (var info in props)
                {
                    if (Include(info))
                    {
                        string? str = null;
                        var obj = info.GetValue(Properties);

                        if (obj is IConvertible con)
                        {
                            str = con?.ToString(ScreenIO.Culture);
                        }
                        else
                        {
                            str = obj?.ToString();
                        }

                        var text = info.Name + " = ";

                        if (str == null)
                        {
                            text += NullString;
                        }
                        else
                        if (str.Length == 0)
                        {
                            text += EmptyString;
                        }
                        else
                        {
                            text += str;
                        }

                        if (IsReadOnly)
                        {
                            n += 1;
                            Menu.Add(new MenuItem(text.ToString(), ColorId.Text));
                        }
                        else
                        if (info.GetSetMethod(false) != null)
                        {
                            Menu.Add(new MenuItem(n++, text.ToString(), PropertyHandler, info));
                        }
                        else
                        {
                            Menu.Add(new MenuItem(n++, text.ToString(), null, info));
                        }

                        hasHelp |= GetPropertyDesc(info) != null;
                    }
                }

                if (n == 1)
                {
                    Menu.Add(new MenuItem("[None]", ColorId.Text));
                }

                if (ComplexProperties.Count != 0)
                {
                    Menu.Add(null);

                    foreach (var item in ComplexProperties)
                    {
                        Menu.Add(item);
                    }
                }

                if (ShowConfirmChanges || (hasHelp && ShowHelp))
                {
                    Menu.Add(null);
                    Menu.Add(new MenuItem("Options"));

                    if (ShowConfirmChanges)
                    {
                        Menu.Add(new MenuItem(ConfirmShortcut, ComfirmChangesText, IsModified ? AcceptHandler : null));
                    }

                    if (hasHelp && ShowHelp)
                    {
                        Menu.Add(new MenuItem("H", "Show Help", HelpHandler));
                    }
                }
            }

            base.OnPrintStarted();
        }

        /// <summary>
        /// Overrides.
        /// </summary>
        protected override void PrintMain()
        {
            if (_isHelpScreen)
            {
                var props = Properties.GetType().GetProperties();
                Array.Sort(props, new NameComparer());

                foreach (var info in props)
                {
                    if (Include(info))
                    {
                        ScreenIO.PrintLn();
                        ScreenIO.Print(info.Name + " [" + info.PropertyType.Name + "]: ");
                        ScreenIO.PrintLn(GetPropertyDesc(info) ?? "No information available.", ColorId.Gray);
                    }
                }
            }

            base.PrintMain();
        }

        /// <summary>
        /// Override to intercept back key to confirm whether changes should be abandoned. Only
        /// applied if <see cref="ConfirmAbandon"/> is true.
        /// </summary>
        protected override string? InputPrompt()
        {
            var rslt = base.InputPrompt();

            if (ConfirmAbandon && IsModified &&
                (InputEquals(BackMenuOption?.Item1, rslt) || InputEquals(HomeMenuOption?.Item1, rslt)))
            {
                ScreenIO.PrintLn();
                var prompt = new Prompter(PromptStyle.Confirm);
                prompt.Prefix = "Abandon changes? [%Y%/%N%]: ";

                if (prompt.Execute() == PromptStatus.Yes)
                {
                    return rslt;
                }

                // Cause reprint
                return "";
            }

            return rslt;
        }

        private static string? GetPropertyDesc(PropertyInfo info)
        {
            var desc = (DescriptionAttribute?)Attribute.GetCustomAttribute(info, typeof(DescriptionAttribute), true);

            if (desc != null && !string.IsNullOrEmpty(desc.Description))
            {
                return desc.Description;
            }

            return null;
        }

        private PageLogic PropertyHandler(MenuItem sender)
        {
            var info = (PropertyInfo)(sender.Tag ?? throw new ArgumentNullException(nameof(sender.Tag)));
            var type = info.PropertyType;
            bool flags = type.IsEnum == true && type.GetCustomAttributes<FlagsAttribute>().Any();

            if (ShowHelp)
            {
                var desc = GetPropertyDesc(info);

                if (desc != null)
                {
                    ScreenIO.PrintLn();
                    ScreenIO.Print(HelpPrefix);
                    ScreenIO.PrintLn(desc, ColorId.Gray);
                }
            }

            ScreenIO.PrintLn();

            var prompt = new Prompter(type);
            prompt.Prefix = $"{info.Name}? [{type.Name}]: ";

            if (ShowEnumsOnPrompt && type.IsEnum)
            {
                var sb = new StringBuilder();
                ScreenIO.Print(flags ? "Combination of: " : "One of: ");

                foreach (var name in Enum.GetNames(type))
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(prompt.Culture.TextInfo.ListSeparator);
                        sb.Append(" ");
                    }

                    sb.Append(name);
                }

                ScreenIO.PrintLn(sb.ToString(), ColorId.Gray);
                ScreenIO.PrintLn();
            }

            while (true)
            {
                if (prompt.Execute() != PromptStatus.Entered)
                {
                    return PageLogic.Reprint;
                }

                if (prompt.TryResult(type, out object? value))
                {
                    if (type == typeof(string) && string.IsNullOrWhiteSpace((string?)value) && EmptyToNull)
                    {
                        value = null;
                    }

                    info.SetValue(Properties, value);

                    IsModified = true;
                    return PageLogic.Reprint;
                }

                ScreenIO.Print("Invalid input: ", ColorId.Warning);
                ScreenIO.PrintLn($"Not a valid {type.Name} value");
                ScreenIO.PrintLn();
            }
        }

        private PageLogic AcceptHandler(MenuItem _)
        {
            if (OnConfirmed())
            {
                IsConfirmed = true;
                return PageLogic.ExitToParent;
            }

            return PageLogic.Reprint;
        }

        private PageLogic HelpHandler(MenuItem _)
        {
            new PrimitivePropertiesPage<T>(this, Properties, "Help").Execute();
            return PageLogic.Reprint;
        }

        private bool Include(PropertyInfo info)
        {
            var showReadonly = ShowReadonlyProperties || IsReadOnly;

            if (info.GetGetMethod(false) == null || (!showReadonly && info.GetSetMethod(false) == null))
            {
                return false;
            }

            if (Exclusions.Contains(info.Name))
            {
                return false;
            }

            if (Inclusions.Count != 0 && !Inclusions.Contains(info.Name))
            {
                return false;
            }

            return info.PropertyType == typeof(string) || typeof(IConvertible).IsAssignableFrom(info.PropertyType);
        }

        private class NameComparer : IComparer<PropertyInfo>
        {
            public int Compare(PropertyInfo? x, PropertyInfo? y)
            {
                return string.CompareOrdinal(x?.Name, y?.Name);
            }
        }
    }
}