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
    /// A result type for <see cref="CathodeRayPage.LogicHandler"/> method.
    /// methods.
    /// </summary>
    public enum PageLogic
    {
        /// <summary>
        /// Reprint the page. Does not exit the page, but waits again on the input prompt. This is the
        /// usual return value of <see cref="CathodeRayPage.LogicHandler"/> when the input string was handled.
        /// </summary>
        Reprint = 0,

        /// <summary>
        /// Indicates that the user input string was unrecognised by the <see cref="CathodeRayPage.LogicHandler"/>
        /// handler. This causes <see cref="CathodeRayPage"/> to display an "INVALID INPUT" message and
        /// prompt the user again. The page execution does not exit.
        /// </summary>
        Unknown,

        /// <summary>
        /// Equivalent to <see cref="Unknown"/> but does not display any "INVALID INPUT" message.
        /// The prompt is simply shown again. This is only useful in custom prompt handlers.
        /// </summary>
        Reprompt,

        /// <summary>
        /// Exit page execution. Execution will fall back to the parent page (previous page).
        /// </summary>
        ExitToParent,

        /// <summary>
        /// Exit all child page execution. Execution will fall back to the root page (home).
        /// </summary>
        ExitToRoot,

        /// <summary>
        /// Exit the entire page execution chain, including the root page <see cref="CathodeRayPage.Execute"/>
        /// method. Typically, the application would return from its Main() method on return.
        /// </summary>
        ExitAll,
    }
}
