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
    /// Provides IDs which refer to context rather than the color itself. These color IDs are mapped
    /// to ConsoleColor by the <see cref="Theme"/> class.
    /// </summary>
    public enum ColorId
    {
        /// <summary>
        /// The background color.
        /// </summary>
        Background,

        /// <summary>
        /// Primary foreground text.
        /// </summary>
        Text,

        /// <summary>
        /// A foreground color used to highlight page title text. Can be the same as <see cref="Text"/>.
        /// </summary>
        Title,

        /// <summary>
        /// A foreground color used for user input text. Can be the same as <see cref="Text"/>.
        /// </summary>
        Input,

        /// <summary>
        /// An alternative text color semantically equivalent to "bold". While it can be the same as
        /// <see cref="Text"/>, it is typically a brighter variant.
        /// </summary>
        High,

        /// <summary>
        /// A foreground gray color used often used to indicate that an option is disabled, but
        /// can also be used to display text.
        /// </summary>
        Gray,

        /// <summary>
        /// A foreground color used to highlight a successful status. Typically green or dark green.
        /// </summary>
        Success,

        /// <summary>
        /// A foreground color used to highlight a warning status. Typically yellow or dark yellow.
        /// </summary>
        Warning,

        /// <summary>
        /// A foreground color used to highlight a critical error or failure. Typically red.
        /// </summary>
        Critical,
    }
}
