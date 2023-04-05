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
    /// Allows the user to select a shell color theme.
    /// </summary>
    public class ThemePage : CathodeRayPage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ThemePage(CathodeRayPage parent, string? title = "Theme Colors")
            : base(parent, title)
        {
            int n = 1;

            foreach (var tag in Theme.Themes)
            {
                Menu.Add(new MenuItem(n++, tag.Name, ThemeHandler, tag));
            }
        }

        /// <summary>
        /// Gets or set whether to show samples for theme colors.
        /// </summary>
        public bool ShowSample { get; set; } = false;

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnPrintStarted"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            foreach (var m in Menu)
            {
                if (m != null)
                {
                    m.IsSuffixed = ScreenIO.Theme.Name == m.Text;
                }
            }

            base.OnPrintStarted();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.PrintMain"/>.
        /// </summary>
        protected override void PrintMain()
        {
            if (ShowSample)
            {
                ScreenIO.PrintLn();

                ScreenIO.Print("Current: ", ColorId.Gray);
                ScreenIO.PrintLn(ScreenIO.Theme.Name);

                ScreenIO.Print("Primary: ", ColorId.Gray);
                ScreenIO.Print(ColorId.Title.ToString() + ", ", ColorId.Title);
                ScreenIO.Print(ColorId.Input.ToString() + ", ", ColorId.Input);
                ScreenIO.Print(ColorId.High.ToString() + ", ", ColorId.High);
                ScreenIO.Print(ColorId.Gray.ToString(), ColorId.Gray);
                ScreenIO.PrintLn();

                ScreenIO.Print("Status:  ", ColorId.Gray);
                ScreenIO.Print(ColorId.Success.ToString() + ", ", ColorId.Success);
                ScreenIO.Print(ColorId.Warning.ToString() + ", ", ColorId.Warning);
                ScreenIO.Print(ColorId.Critical.ToString(), ColorId.Critical);
            }

            base.PrintMain();
        }

        private PageLogic ThemeHandler(MenuItem sender)
        {
            ScreenIO.Theme = (Theme)(sender.Tag ?? new Theme());
            return PageLogic.Reprint;
        }
    }
}
