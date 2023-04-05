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
    /// Option for use with <see cref="ScreenIO.Truncate(string, int, Truncation)"/>.
    /// </summary>
    public enum Truncation
    {
        /// <summary>
        /// The text is truncated at the specified width.
        /// </summary>
        Simple,

        /// <summary>
        /// If the truncation width is greater than 3, text which is truncated terminates with "...".
        /// </summary>
        EllipsesEnd,

        /// <summary>
        /// If the truncation width is greater than 3, text which is truncated is shortened by
        /// replacing a number of characters in the center with "...".
        /// </summary>
        EllipsesCenter
    }
}
