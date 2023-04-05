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
    /// Formatting flags for use with the <see cref="ScreenIO"/> class. Where flags are mutually
    /// exclusive, it is the convention that lower values take precedence.
    /// </summary>
    [Flags]
    public enum ScreenOptions
    {
        /// <summary>
        /// None. Left-aligned without wrapping, but allows other flags to take
        /// precedence.
        /// </summary>
        None = 0x0000,

        // 0x0001 Reserved

        /// <summary>
        /// The text is horizontally centered in the remaining space.
        /// </summary>
        Center = 0x0002,

        /// <summary>
        /// The text right-aligned to the remaining space.
        /// </summary>
        Right = 0x0004,

        /// <summary>
        /// Case is not transformed. This flag takes precedence over other case flags.
        /// </summary>
        NoCase = 0x0008,

        /// <summary>
        /// The text is printed in uppercase.
        /// </summary>
        Uppercase = 0x0010,

        /// <summary>
        /// The text is printed in lowercase.
        /// </summary>
        Lowercase = 0x0020,

        /// <summary>
        /// Text is left to the Console window to wrap. This flag takes precedence over other
        /// wrapping flags.
        /// </summary>
        NoWrap = 0x0040,

        /// <summary>
        /// The text is wrapped, allowing for word breaks.
        /// </summary>
        WordWrap = 0x0080,

        /// <summary>
        /// The text is block wrapped. No allowance is made for word breaks.
        /// </summary>
        BlockWrap = 0x0100,
    }

    /// <summary>
    /// Extension methods for <see cref="ScreenOptions"/>.
    /// </summary>
    public static class ScreenOptionsExtensions
    {
        /// <summary>
        /// Returns: <see cref="ScreenOptions.Center"/>, <see cref="ScreenOptions.Right"/> or
        /// <see cref="ScreenOptions.None"/>.
        /// </summary>
        public static ScreenOptions Alignment(this ScreenOptions value)
        {
            if (value.HasFlag(ScreenOptions.Center))
            {
                return ScreenOptions.Center;
            }

            if (value.HasFlag(ScreenOptions.Right))
            {
                return ScreenOptions.Right;
            }

            return ScreenOptions.None;
        }

        /// <summary>
        /// Returns: <see cref="ScreenOptions.Uppercase"/>, <see cref="ScreenOptions.Lowercase"/> or
        /// <see cref="ScreenOptions.None"/>. If the value has the flag
        /// <see cref="ScreenOptions.NoCase"/>, the result is <see cref="ScreenOptions.None"/>.
        /// </summary>
        public static ScreenOptions TextCase(this ScreenOptions value)
        {
            if (!value.HasFlag(ScreenOptions.NoCase))
            {
                if (value.HasFlag(ScreenOptions.Uppercase))
                {
                    return ScreenOptions.Uppercase;
                }

                if (value.HasFlag(ScreenOptions.Lowercase))
                {
                    return ScreenOptions.Lowercase;
                }
            }

            return ScreenOptions.None;
        }

        /// <summary>
        /// Returns: <see cref="ScreenOptions.BlockWrap"/>, <see cref="ScreenOptions.WordWrap"/> or
        /// <see cref="ScreenOptions.None"/>. If the value has the flag
        /// <see cref="ScreenOptions.NoWrap"/>, the result is <see cref="ScreenOptions.None"/>.
        /// </summary>
        public static ScreenOptions Wrapping(this ScreenOptions value)
        {
            if (!value.HasFlag(ScreenOptions.NoWrap))
            {
                if (value.HasFlag(ScreenOptions.WordWrap))
                {
                    return ScreenOptions.WordWrap;
                }

                if (value.HasFlag(ScreenOptions.BlockWrap))
                {
                    return ScreenOptions.BlockWrap;
                }
            }

            return ScreenOptions.None;
        }

    }
}
