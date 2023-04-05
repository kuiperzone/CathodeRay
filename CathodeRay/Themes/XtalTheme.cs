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

using System;

namespace KuiperZone.CathodeRay.Themes
{
    /// <summary>
    /// Derives from <see cref="Theme"/> and overrides the default colors.
    /// This theme shows light-gray foreground text on a blue background.
    /// </summary>
    public class XtalTheme : Theme
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public XtalTheme()
            : base()
        {
            Name = "XTAL";
            this[ColorId.Text] = ConsoleColor.Gray;
            this[ColorId.Background] = ConsoleColor.DarkBlue;

            this[ColorId.Title] = ConsoleColor.White;
            this[ColorId.Input] = ConsoleColor.Gray;
            this[ColorId.High] = ConsoleColor.White;
            this[ColorId.Gray] = ConsoleColor.DarkGray;

            this[ColorId.Success] = ConsoleColor.Green;
            this[ColorId.Warning] = ConsoleColor.Yellow;
            this[ColorId.Critical] = ConsoleColor.Red;
        }

    }
}
