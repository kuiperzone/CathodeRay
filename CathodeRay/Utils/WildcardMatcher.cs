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

using System.Text.RegularExpressions;

namespace KuiperZone.CathodeRay.Utils
{
    /// <summary>
    /// Wildcard pattern matcher class.
    /// </summary>
    public class WildcardMatcher
    {
        // https://stackoverflow.com/questions/188892/glob-pattern-matching-in-net
        private readonly Regex? _regex;

        /// <summary>
        /// Constructor. The "pattern" value of null or empty is translated to "*". The "compiled" may be
        /// set to true if the filter is going to be used multiple times (generally it should be false).
        /// </summary>
        public WildcardMatcher(string? patten, bool ignoreCase = false, bool compiled = false)
        {
            Pattern = string.IsNullOrEmpty(patten) ? "*" : patten;
            IgnoreCase = ignoreCase;
            Compiled = compiled;

            if (Pattern != "*" && patten != null)
            {
                var opts = RegexOptions.Singleline;

                if (IgnoreCase)
                {
                    opts |= RegexOptions.IgnoreCase;
                }

                if (Compiled)
                {
                    opts |= RegexOptions.Compiled;
                }

                _regex = new Regex("^" + Regex.Escape(patten).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", opts);
            }
        }

        /// <summary>
        /// Gets the wildcard pattern string.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Gets true if case is ignored.
        /// </summary>
        public bool IgnoreCase { get; }

        /// <summary>
        /// Gets true if the internal Regex is compiled.
        /// </summary>
        public bool Compiled { get; }

        /// <summary>
        /// Returns true if "s" matches <see cref="Pattern"/>. Always returns false if null.
        /// </summary>
        public bool IsMatch(string? s)
        {
            if (s == null)
            {
                return false;
            }

            if (_regex == null)
            {
                // Equivalent to "*"
                return true;
            }

            return _regex.IsMatch(s);
        }
    }
}
