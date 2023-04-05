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
    /// Option flags.
    /// </summary>
    [Flags]
    public enum PageOptions
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Page to show the <see cref="CathodeRayPage.PageTitle"/> at the top.
        /// </summary>
        ShowTitle = 0x0001,

        /// <summary>
        /// If specified, pages shows <see cref="CathodeRayPage.QualifiedTitle"/> instead of
        /// <see cref="CathodeRayPage.PageTitle"/>.
        /// </summary>
        ShowQualifiedTitle = ShowTitle | 0x0002,

        /// <summary>
        /// If <see cref="ShowQualifiedTitle"/> is specified, this option causes the root part to
        /// be omitted. Instead, the title sequence will start with the first child title.
        /// </summary>
        SkipRootTitle = 0x0004,

        /// <summary>
        /// If <see cref="ShowQualifiedTitle"/> is specified but the fully qualified title exceeds
        /// <see cref="ScreenIO.ActualWidth"/>, this option causes the title to revert to
        /// <see cref="CathodeRayPage.PageTitle"/>.
        /// </summary>
        RevertPageTitleIfQualifiedIsLong = 0x0008,

        /// <summary>
        /// If not specified, each page appends its print output to the terminal window. If this
        /// flag is specified, the screen is cleared each time a page prints.
        /// </summary>
        AutoCls = 0x0010,

        /// <summary>
        /// Indents menu options by <see cref="ScreenIO.TabSize"/>.
        /// </summary>
        IndentMenu = 0x0020,

        /// <summary>
        /// A default option combination.
        /// </summary>
        Default = ShowQualifiedTitle | SkipRootTitle | AutoCls
    }
}
