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
    /// The input style used with the <see cref="Prompter"/> class.
    /// </summary>
    public enum PromptStyle
    {
        /// <summary>
        /// Waits for a single key with hidden input. Additionally, the prompt prefix is erased
        /// from the screen and the cursor position left unchanged on return.
        /// The default prompt prefix is "Press any key ... ".
        /// </summary>
        AnyKey,

        /// <summary>
        /// Allows the user to input a line of text followed by the return key. The variable
        /// "%MAXLEN%" can be used in the prompt prefix as a placeholder for the <see
        /// cref="Prompter.MaxLength"/> value. The default prompt prefix is "Input?: ".
        /// </summary>
        Text,

        /// <summary>
        /// Similar to <see cref="Text"/> except that default prompt prefix is "Password: " and "*"
        /// characters are shown for input text. Input string of this kind are NOT stored in the
        /// internal history buffer.
        /// </summary>
        HidePassword,

        /// <summary>
        /// Same as <see cref="HidePassword"/> except that input characters are shown. Input string of
        /// this kind are NOT stored in the internal history buffer.
        /// </summary>
        ShowPassword,

        /// <summary>
        /// Same as <see cref="Text"/>, but prevents the user from inputting invalid filename
        /// characters such as "?". Additionally, the result string will be trimmed left and right. The
        /// default prompt prefix is "Filename?: ".
        /// </summary>
        FileName,

        /// <summary>
        /// Same as <see cref="FileName"/> but allows path separator characters, namely "/" and "\".
        /// The default prompt prefix is "Path?: ".
        /// </summary>
        FilePath,

        /// <summary>
        /// Allows the user to select between two options, pertaining to "yes" and "no". The user is
        /// also allowed to press Escape (abort). Allowed value strings can be configured using <see
        /// cref="Prompter.YesValue"/> and <see cref="Prompter.NoValue"/> properties. The default
        /// prompt prefix is "Confirm? [%Y%/%N%]: ", where the variables "%Y%" and "%N%" act
        /// as placeholders.
        /// </summary>
        Confirm,
    }

    /// <summary>
    /// Extension methods for <see cref="PromptStyle"/>.
    /// </summary>
    public static class PromptStyleExtensions
    {
        /// <summary>
        /// Returns true if the style expects text input. It excludes <see cref="PromptStyle.AnyKey"/>
        /// and <see cref="PromptStyle.Confirm"/>.
        /// </summary>
        public static bool IsText(this PromptStyle s)
        {
            return s == PromptStyle.Text || IsPassword(s) || IsPath(s);
        }

        /// <summary>
        /// Returns true if one of the password styles.
        /// </summary>
        public static bool IsPassword(this PromptStyle s)
        {
            return s == PromptStyle.HidePassword || s == PromptStyle.ShowPassword;
        }

        /// <summary>
        /// Returns true if one of the filename or path styles.
        /// </summary>
        public static bool IsPath(this PromptStyle s)
        {
            return s == PromptStyle.FileName || s == PromptStyle.FilePath;
        }

    }
}
