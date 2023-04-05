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
    /// Class suitable for JSON serialization which contains a subset of settings for <see cref="ScreenIO"/>
    /// and <see cref="CathodeRayPage"/>.
    /// </summary>
    public class ScreenSettings
    {
        /// <summary>
        /// Default constructor sets properties from <see cref="ScreenIO"/> properties.
        /// </summary>
        public ScreenSettings()
        {
            ThemeName = ScreenIO.Theme.Name;
            TransparentBackground = ScreenIO.TransparentBackground;
            FormatWidth = ScreenIO.FormatWidth;
            ScreenCenter = ScreenIO.Options.HasFlag(ScreenOptions.Center);
            ScrollBreak = ScreenIO.ScrollBreak;
            AutoCls = CathodeRayPage.GlobalOptions.HasFlag(PageOptions.AutoCls);
        }

        /// <summary>
        /// Gets or sets the <see cref="Theme"/>.
        /// </summary>
        public string? ThemeName { get; set; }

        /// <summary>
        /// Gets or sets whether to print the background color.
        /// </summary>
        public bool TransparentBackground { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ScreenIO.FormatWidth"/> value.
        /// </summary>
        public int FormatWidth { get; set; }

        /// <summary>
        /// Gets or sets whether content is to be displayed in the screen center.
        /// </summary>
        public bool ScreenCenter { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ScreenIO.ScrollBreak"/> value.
        /// </summary>
        public bool ScrollBreak { get; set; }

        /// <summary>
        /// Gets whether page reprints clear the screen. The default is true.
        /// </summary>
        public bool AutoCls { get; set; } = true;

        /// <summary>
        /// Applies the settings to <see cref="ScreenIO"/>. Does not apply those pertaining to <see cref="CathodeRayPage"/>.
        /// </summary>
        public void Apply()
        {
            ScreenIO.Theme = Theme.Get(ThemeName);
            ScreenIO.TransparentBackground = TransparentBackground;
            ScreenIO.FormatWidth = FormatWidth;
            ScreenIO.ScrollBreak = ScrollBreak;

            if (ScreenCenter) ScreenIO.Options |= ScreenOptions.Center;
            else ScreenIO.Options &= ~ScreenOptions.Center;

            if (AutoCls) CathodeRayPage.GlobalOptions |= PageOptions.AutoCls;
            else CathodeRayPage.GlobalOptions &= ~PageOptions.AutoCls;
        }

    }
}
