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
    /// Designates the return value of <see cref="Prompter.Execute"/>.
    /// </summary>
    public enum PromptStatus
    {
        /// <summary>
        /// The <see cref="Prompter.Execute"/> is waiting to be called.
        /// </summary>
        Waiting,

        /// <summary>
        /// The Escape key was pressed. The <see cref="Prompter.InputString"/> value is null.
        /// </summary>
        Escaped,

        /// <summary>
        /// The Enter key was pressed. The <see cref="Prompter.InputString"/> value is not null, but
        /// may contain and empty or whitespace string.
        /// </summary>
        Entered,

        /// <summary>
        /// The <see cref="Prompter.Style"/> is <see cref="PromptStyle.Confirm"/> and user entered
        /// the <see cref="Prompter.YesValue"/> string.
        /// </summary>
        Yes,

        /// <summary>
        /// The <see cref="Prompter.Style"/> is <see cref="PromptStyle.Confirm"/> and user entered
        /// the <see cref="Prompter.NoValue"/> string.
        /// </summary>
        No,
    }
}
