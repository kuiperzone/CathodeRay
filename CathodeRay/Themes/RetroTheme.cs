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

namespace KuiperZone.CathodeRay.Themes
{
    /// <summary>
    /// Derives from <see cref="Theme"/> and overrides the default colors.
    /// This theme shows green foreground text on a black background.
    /// </summary>
    public class RetroTheme : Theme
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RetroTheme(bool bright = false)
        {
            if (!bright)
            {
                Name = "RETRO";
                this[ColorId.Text] = ConsoleColor.DarkGreen;
                this[ColorId.Background] = ConsoleColor.Black;

                this[ColorId.Title] = ConsoleColor.Gray;
                this[ColorId.Input] = ConsoleColor.Gray;
                this[ColorId.High] = ConsoleColor.Green;
                this[ColorId.Gray] = ConsoleColor.DarkGray;

                this[ColorId.Success] = ConsoleColor.DarkGreen;
                this[ColorId.Warning] = ConsoleColor.DarkYellow;
                this[ColorId.Critical] = ConsoleColor.Red;
            }
            else
            {
                Name = "RETRO Bright";
                this[ColorId.Text] = ConsoleColor.Green;
                this[ColorId.Background] = ConsoleColor.Black;

                this[ColorId.Title] = ConsoleColor.Gray;
                this[ColorId.Input] = ConsoleColor.Gray;
                this[ColorId.High] = ConsoleColor.White;
                this[ColorId.Gray] = ConsoleColor.DarkGray;

                this[ColorId.Success] = ConsoleColor.Green;
                this[ColorId.Warning] = ConsoleColor.Yellow;
                this[ColorId.Critical] = ConsoleColor.Red;
            }
        }

    }
}
