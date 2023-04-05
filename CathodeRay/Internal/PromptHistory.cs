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

using System.Globalization;
using System.Text;
using KuiperZone.CathodeRay.Utils;

namespace KuiperZone.CathodeRay.Internal
{
    /// <summary>
    /// Used with <see cref="Prompter"/> and outsources some the logic complexity.
    /// </summary>
    internal class PromptHistory
    {
        private static readonly char[] IllegalPathChars = GetIllegalPathChars(false);
        private static readonly char[] IllegalFilenameChars = GetIllegalPathChars(true);

        /// <summary>
        /// Maximum static history items.
        /// </summary>
        public const int MaxHistory = 32;

        private static readonly List<string> s_history = new List<string>();

        private int _historyIdx = 0;
        private string? _combinedLegal;
        private bool _forwardLast;
        public PromptHistory(PromptStyle style)
        {
            Style = style;
            BufferHistory = Style.IsText() && !Style.IsPassword();

            if (Style.IsPath())
            {
                MinLength = 1;
            }
        }

        public PromptStyle Style { get; }
        public bool BufferHistory { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; } = 255;
        public string? LegalChars { get; set; }
        public string? LegalFilter { get; set; }
        public bool IgnoreLegalCase { get; set; }
        public bool DenySpace { get; set; }
        public string YesValue { get; set; } = "y";
        public string NoValue { get; set; } = "N";
        public Type ValueType { get; set; } = typeof(string);
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        public bool TryConvert(Type type, string? str, out object? value)
        {
            value = null;

            if (str == null || type == null)
            {
                return false;
            }

            if (type == typeof(string))
            {
                value = str;
                return true;
            }

            var trm = str.Trim();

            if (type == typeof(bool))
            {
                if (trm.Equals("true", StringComparison.OrdinalIgnoreCase) || trm == "1")
                {
                    value = true;
                    return true;
                }

                if (trm.Equals("false", StringComparison.OrdinalIgnoreCase) || trm == "0")
                {
                    value = false;
                    return true;
                }

                return false;
            }

            if (trm.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                const NumberStyles ns = NumberStyles.HexNumber;
                var hex = trm.Substring(2);

                if (type == typeof(int) && int.TryParse(hex, ns, Culture, out int i32))
                {
                    value = i32;
                    return true;
                }

                if (type == typeof(uint) && uint.TryParse(hex, ns, Culture, out uint u32))
                {
                    value = u32;
                    return true;
                }

                if (type == typeof(long) && long.TryParse(hex, ns, Culture, out long i64))
                {
                    value = i64;
                    return true;
                }

                if (type == typeof(ulong) && ulong.TryParse(hex, ns, Culture, out ulong u64))
                {
                    value = u64;
                    return true;
                }

                if (type == typeof(short) && short.TryParse(hex, ns, Culture, out short i16))
                {
                    value = i16;
                    return true;
                }

                if (type == typeof(ushort) && ushort.TryParse(hex, ns, Culture, out ushort u16))
                {
                    value = u16;
                    return true;
                }

                if (type == typeof(sbyte) && sbyte.TryParse(hex, ns, Culture, out sbyte i8))
                {
                    value = i8;
                    return true;
                }

                if (type == typeof(byte) && byte.TryParse(hex, ns, Culture, out byte u8))
                {
                    value = u8;
                    return true;
                }
            }

            if (type.IsEnum && Enum.TryParse(type, trm, true, out value))
            {
                // Insensitive
                return true;
            }

            try
            {
                value = Convert.ChangeType(trm, type, Culture);
                return true;
            }
            catch
            {
            }

            return false;
        }

        public string? ForwardHistory()
        {
            int temp = _historyIdx + 1;

            if (temp > -1)
            {
                if (temp < s_history.Count)
                {
                    _historyIdx = temp;
                    return s_history[temp];
                }

                if (!_forwardLast)
                {
                    // Allow last down to clear
                    _forwardLast = true;
                    return "";
                }
            }

            return null;
        }

        public string? BackHistory()
        {
            _forwardLast = false;

            int temp = _historyIdx - 1;
            if (temp > -1 && temp < s_history.Count)
            {
                _historyIdx = temp;
                return s_history[temp];
            }

            return null;
        }

        public void Reset()
        {
            if (MinLength > MaxLength)
            {
                throw new InvalidOperationException(nameof(MinLength) + " value cannot be greater than " + nameof(MaxLength));
            }

            if (string.Equals(YesValue, NoValue, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException(nameof(YesValue) + " and " + nameof(NoValue) + " values cannot be equal");
            }

            _forwardLast = false;
            _historyIdx = s_history.Count;
            _combinedLegal = CombineLegalChars(LegalChars, LegalFilter);
        }

        /// <summary>
        /// Returns the string if the "buffer" string matches the accept rules and, if accepted,
        /// pushes the string onto an internal static history. On failure, it returns null.
        /// On success, it returns the string to assign to <see cref="Prompter.InputString"/>.
        /// </summary>
        public string? AcceptCommit(string? buffer)
        {
            if (buffer == null)
            {
                return null;
            }

            // Don't lose buffer
            string trm = buffer.Trim();

            if (Style == PromptStyle.Confirm)
            {
                // Do not add to history
                if (trm.Equals(YesValue, StringComparison.OrdinalIgnoreCase))
                {
                    AddHistory(YesValue);
                    return YesValue;
                }

                if (trm.Equals(NoValue, StringComparison.OrdinalIgnoreCase))
                {
                    AddHistory(NoValue);
                    return NoValue;
                }

                return null;
            }

            if (DenySpace && trm.Contains(' '))
            {
                return null;
            }

            if (Style == PromptStyle.FileName)
            {
                foreach (var x in IllegalFilenameChars)
                {
                    if (trm.Contains(x))
                    {
                        return null;
                    }
                }
            }
            else
            if (Style == PromptStyle.FilePath)
            {
                foreach (var x in IllegalPathChars)
                {
                    if (trm.Contains(x))
                    {
                        return null;
                    }
                }
            }

            if (_combinedLegal != null)
            {
                foreach (var x in buffer)
                {
                    if (IgnoreLegalCase && !_combinedLegal.Contains(x, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return null;
                    }

                    if (!IgnoreLegalCase && !_combinedLegal.Contains(x))
                    {
                        return null;
                    }
                }
            }

            if (new WildcardMatcher(LegalFilter, IgnoreLegalCase).IsMatch(buffer))
            {
                if (Style.IsPath())
                {
                    // Use trimmed length for these
                    buffer = trm;
                }

                int length = buffer.Length;

                if (length >= MinLength && length <= MaxLength && TryConvert(ValueType, buffer, out _))
                {
                    // Accept
                    AddHistory(buffer);
                    return buffer;
                }
            }

            return null;
        }

        private void AddHistory(string str)
        {
            if (BufferHistory && Style.IsText() && !string.IsNullOrWhiteSpace(str))
            {
                // To history
                s_history.Add(str);

                if (s_history.Count > MaxHistory)
                {
                    s_history.RemoveAt(0);
                }

                _forwardLast = false;
                _historyIdx = s_history.Count;
            }
        }

        private string? CombineLegalChars(string? legal, string? allowFilter)
        {
            if (!string.IsNullOrEmpty(legal) && !string.IsNullOrEmpty(allowFilter))
            {
                var sb = new StringBuilder(legal);

                foreach (var c in allowFilter)
                {
                    if (c != '*' && c != '?')
                    {
                        sb.Append(c);
                    }
                }

                return sb.ToString();
            }

            return legal;
        }

        private static char[] GetIllegalPathChars(bool filename)
        {
            // NB. On .NET Core 2.2, GetInvalidPathChars() was found to be allowing "<>*?:".
            // So we define our own base sequence for consistency at utility results to them.
            char[] others;
            var list = new List<char>(new char[] { '"', '<', '>', '|', ':', '*', '?', '&' });

            if (filename)
            {
                list.Add('/');
                list.Add('\\');
                others = Path.GetInvalidFileNameChars();
            }
            else
            {
                others = Path.GetInvalidPathChars();
            }

            foreach (var c in others)
            {
                if (c > 0x20 && !list.Contains(c))
                {
                    list.Add(c);
                }
            }

            return list.ToArray();
        }

    }
}
