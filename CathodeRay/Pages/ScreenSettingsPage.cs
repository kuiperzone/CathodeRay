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

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// Allows the user to modify certain <see cref="ScreenIO"/> and <see cref="CathodeRayPage"/> settings,
    /// including the color theme. Changes are applied to this class instance, and all ancestors including the
    /// root page.
    /// </summary>
    public class ScreenSettingsPage : CathodeRayPage
    {
        private static readonly int[] StdWidths = new int[] { 80, 100, 120, 200 };

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScreenSettingsPage(CathodeRayPage parent, string? title = "Screen Settings")
            : base(parent, title)
        {
        }

        /// <summary>
        /// Gets or sets whether theme options are shown.
        /// </summary>
        public bool ShowThemes { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show the "Screen Center" option.
        /// </summary>
        public bool ShowCenter { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show the "Auto CLS" option.
        /// </summary>
        public bool ShowAutoCls { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show the "Scroll More" option.
        /// </summary>
        public bool ShowScrollBreak { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show options to change the output width.
        /// </summary>
        public bool ShowPageWidth { get; set; } = true;

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnPrintStarted"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            int n = 1;
            Menu.Clear();

            if (ShowThemes)
            {
                Menu.Add(new MenuItem(n++, "Themes", ThemeHandler));
                Menu.Add(new MenuItem(n++, "Transparent Background", ScreenIO.TransparentBackground, TransparentHandler));
            }

            if (ShowCenter)
            {
                Menu.Add(new MenuItem(n++, "Screen Center", ScreenIO.Options.HasFlag(ScreenOptions.Center), CenterHandler));
            }

            if (ShowAutoCls)
            {
                Menu.Add(new MenuItem(n++, "Auto CLS", GlobalOptions.HasFlag(PageOptions.AutoCls), AutoClsHandler));
            }

            if (ShowScrollBreak)
            {
                Menu.Add(new MenuItem(n++, "Scroll Break", ScreenIO.ScrollBreak, ScrollBreakHandler));
            }

            if (ShowPageWidth)
            {
                Menu.Add(null);
                Menu.Add(new MenuItem("Page Width"));

                int width = ScreenIO.FormatWidth;
                Menu.Add(new MenuItem(n++, "Screen", width == 0, PrintWidthHandler));

                bool sufx = width > 0;

                foreach (var sw in StdWidths)
                {
                    sufx &= width != sw;
                    Menu.Add(new MenuItem(n++, sw.ToString(), width == sw, PrintWidthHandler, sw));
                }

                Menu.Add(new MenuItem(n++, "Custom" + (sufx ? " [" + width + "]" : null), sufx, CustomWidthHandler));
            }

            base.OnPrintStarted();
        }

        private PageLogic ThemeHandler(MenuItem _)
        {
            new ThemePage(this).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic TransparentHandler(MenuItem _)
        {
            ScreenIO.TransparentBackground = !ScreenIO.TransparentBackground;
            return PageLogic.Reprint;
        }

        private PageLogic CenterHandler(MenuItem _)
        {
            bool flip = !ScreenIO.Options.HasFlag(ScreenOptions.Center);

            if (flip) ScreenIO.Options |= ScreenOptions.Center;
            else ScreenIO.Options &= ~ScreenOptions.Center;

            return PageLogic.Reprint;
        }

        private PageLogic AutoClsHandler(MenuItem _)
        {
            bool flip = !GlobalOptions.HasFlag(PageOptions.AutoCls);

            if (flip) GlobalOptions |= PageOptions.AutoCls;
            else GlobalOptions &= ~PageOptions.AutoCls;

            return PageLogic.Reprint;
        }

        private PageLogic ScrollBreakHandler(MenuItem _)
        {
            ScreenIO.ScrollBreak = !ScreenIO.ScrollBreak;
            return PageLogic.Reprint;
        }

        private PageLogic PrintWidthHandler(MenuItem sender)
        {
            ScreenIO.FormatWidth = (int)(sender?.Tag ?? 0);
            return PageLogic.Reprint;
        }

        private PageLogic CustomWidthHandler(MenuItem _)
        {
            const int Min = 40;
            const int Max = 999;

            ScreenIO.PrintLn();
            var prompt = new Prompter();
            prompt.Prefix = $"Custom? [{Min}, {Max}]: ";

            prompt.Execute();

            if (prompt.TryResult(Min, Max, out int value))
            {
                ScreenIO.FormatWidth = value;
            }
            else
            {
                ScreenIO.PrintPause("Out of range", ColorId.Warning);
            }

            return PageLogic.Reprint;
        }

    }
}
