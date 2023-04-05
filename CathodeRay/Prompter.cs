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

using System.Diagnostics;
using System.Globalization;
using System.Text;
using KuiperZone.CathodeRay.Internal;

namespace KuiperZone.CathodeRay
{
    /// <summary>
    /// Prompts the user for input according to <see cref="Prompter.Style"/>.
    /// </summary>
    public class Prompter
    {
        private const int FlashDelay = 400;

        private readonly PromptHistory _history;
        private Tuple<int, int> _inputXY;
        private ScreenOptions _format;
        private Stopwatch? _flashWatch;
        private string? _inputString;

        /// <summary>
        /// Constructor with <see cref="PromptStyle"/> and prompt message to use. If "message" is
        /// null, <see cref="Prefix"/> is set to appropriate default value based on the style.
        /// </summary>
        public Prompter(PromptStyle style = PromptStyle.Text)
        {
            _history = new PromptHistory(style);

            Style = style;
            Prefix = GetPrefix(Style, null);

            // Should reset, as prompt will
            // be invisible if print cancelled.
            // We also don't want to break on input.
            ScreenIO.Reset();

            PosXY = ScreenIO.PosXY;
            Culture = ScreenIO.Culture;
            _inputXY = PosXY;
        }

        /// <summary>
        /// Constructor with <see cref="ValueType"/>. The <see cref="Style"/> will be <see
        /// cref="PromptStyle.Text"/>.
        /// [%TYPE%]: ".
        /// </summary>
        /// <exception cref="ArgumentNullException">type</exception>
        /// <exception cref="ArgumentException">Not support IConvertible</exception>
        public Prompter(Type type)
            : this(PromptStyle.Text)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type != typeof(string) && !typeof(IConvertible).IsAssignableFrom(type))
            {
                throw new ArgumentException("Not support " + nameof(IConvertible));
            }

            ValueType = type;
            Prefix = GetPrefix(Style, type);
            _history.ValueType = type;
        }

        /// <summary>
        /// Gets the <see cref="PromptStyle"/> supplied on construction.
        /// </summary>
        public PromptStyle Style { get; }

        /// <summary>
        /// Gets the value <see cref="Type"/> supplied on construction. The default is that of <see cref="string"/>.
        /// </summary>
        public Type ValueType { get; } = typeof(string);

        /// <summary>
        /// Gets or sets whether the user can be move through recent <see cref="Prompter"/> history
        /// and whether the input of the instance is stored. The value is true for <see
        /// cref="PromptStyle.Text"/>, filenames but false for others.
        /// </summary>
        public bool BufferHistory
        {
            get { return _history.BufferHistory; }
            set { _history.BufferHistory = value; }
        }

        /// <summary>
        /// Gets or sets the prompt prefix message. An initial value is assigned on construction.
        /// The following placeholder variables can be used: "%MINLEN%", "%MAXLEN%", "%Y%", "%N%", "%TYPE%".
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Gets or sets the prompt prefix color. The initial value is <see cref="ColorId.Text"/>.
        /// </summary>
        public ColorId Color { get; set; } = ColorId.Text;

        /// <summary>
        /// Gets or sets whether <see cref="PromptStyle.Text"/> accepts a limited range of short-cut
        /// keys such as "Home", "PageUp" and "PageDown". Default is false.
        /// </summary>
        public bool ShortCuts { get; set; }

        /// <summary>
        /// The minimum allowed number of input characters for: <see cref="PromptStyle.Text"/>,
        /// <see cref="PromptStyle.HidePassword"/>, <see cref="PromptStyle.FileName"/> and
        /// <see cref="PromptStyle.FilePath"/>. The default value is 0.
        /// </summary>
        public int MinLength
        {
            get { return _history.MinLength; }
            set { _history.MinLength = value; }
        }

        /// <summary>
        /// Gets or sets the maximum allowed number of input characters for: <see
        /// cref="PromptStyle.Text" />, <see cref="PromptStyle.HidePassword" />, <see
        /// cref="PromptStyle.FileName" /> and <see cref="PromptStyle.FilePath" />. The default
        /// value is 255.
        /// </summary>
        public int MaxLength
        {
            get { return _history.MaxLength; }
            set { _history.MaxLength = value; }
        }

        /// <summary>
        /// A sequence of allowed characters where the user will be prevented from inputting other
        /// characters. It does not apply to the <see cref="PromptStyle.Confirm"/> and <see
        /// cref="PromptStyle.AnyKey"/> styles. The property is ignored if null or empty.
        /// </summary>
        public string? LegalChars
        {
            get { return _history.LegalChars; }
            set { _history.LegalChars = value; }
        }

        /// <summary>
        /// Gets or sets a legal wildcard filter. It may be used, for example, to restrict text
        /// input to an ISO8501 date format as follows: "????-??-??". It can be combined with <see
        /// cref="LegalChars"/> to restrict characters to integer values. Note that characters in
        /// <see cref="LegalFilter"/> which are not '*' or '?' are implicitly allowed even excluded
        /// by <see cref="LegalChars"/>. It does not apply to the <see cref="PromptStyle.Confirm"/>
        /// and <see cref="PromptStyle.AnyKey"/> styles. The property is ignored if the value is
        /// null or empty.
        /// </summary>
        public string? LegalFilter
        {
            get { return _history.LegalFilter; }
            set { _history.LegalFilter = value; }
        }

        /// <summary>
        /// Gets or sets whether <see cref="LegalChars"/> or <see cref="LegalFilter"/> is case
        /// insensitive. The default is false (case sensitive).
        /// </summary>
        public bool IgnoreLegalCase
        {
            get { return _history.IgnoreLegalCase; }
            set { _history.IgnoreLegalCase = value; }
        }

        /// <summary>
        /// Gets or sets whether to deny input of the space character.
        /// </summary>
        public bool DenySpace
        {
            get { return _history.DenySpace; }
            set { _history.DenySpace = value; }
        }

        /// <summary>
        /// Gets of sets the "yes" ("%Y%") value for the <see cref="PromptStyle.Confirm"/> input style.
        /// The value must not be null or empty, and must not equal <see cref="NoValue"/>.
        /// Comparisons are not case sensitive.
        /// </summary>
        public string YesValue
        {
            get { return _history.YesValue; }
            set { _history.YesValue = value; }
        }

        /// <summary>
        /// Gets or sets the "no" ("%N%") value for the <see cref="PromptStyle.Confirm"/> input
        /// style. See <see cref="YesValue"/>.
        /// </summary>
        public string NoValue
        {
            get { return _history.NoValue; }
            set { _history.NoValue = value; }
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> provider to use when parsing inputs and for case
        /// sensitivity. When the instance is created, it is set to the <see cref="ScreenIO.Culture"/>
        /// supplied in the constructor. The property cannot be set to null.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _history.Culture; }
            set { _history.Culture = value ?? throw new ArgumentNullException("value"); }
        }

        /// <summary>
        /// Gets the result of the last call to <see cref="Execute"/>. The initial value is <see cref="PromptStatus.Waiting"/>.
        /// </summary>
        public PromptStatus Status { get; private set; }

        /// <summary>
        /// Gets the user input string generated by the last <see cref="Execute"/> call. The value
        /// is empty initially, and will be empty if the user pressed Escape.
        /// </summary>
        public string InputString
        {
            get { return _inputString ?? ""; }
        }

        /// <summary>
        /// Gets the Console position on construction or the moment <see cref="Execute"/> was last called.
        /// </summary>
        public Tuple<int, int> PosXY { get; private set; }

        /// <summary>
        /// Sets both <see cref="MinLength"/> and <see cref="MaxLength"/> properties.
        /// </summary>
        public void SetMinMaxLength(int min, int max)
        {
            MinLength = min;
            MaxLength = max;
        }

        /// <summary>
        /// Executes the prompt and waits for user input. The "seed" contains an optional initial
        /// value. The prompt is always shown at the current <see cref="Console"/> cursor position.
        /// The return value is a <see cref="PromptStatus"/> value whether the user pressed Enter or
        /// Escape. The input string can be obtained using <see cref="InputString"/>. See also <see cref="PromptStyle"/>.
        /// </summary>
        public PromptStatus Execute(string? seed = null)
        {
            _history.Reset();

            // Hold stuff
            PosXY = ScreenIO.PosXY;
            bool cursor = ScreenIO.IsCursorVisible;
            bool scrollBreak = ScreenIO.ScrollBreak;
            ScreenIO.ScrollBreak = false;

            // Set wrapping
            _format = ScreenOptions.NoWrap;

            if (ScreenIO.Options.Wrapping() != ScreenOptions.None)
            {
                // Substitute block wrap
                _format = ScreenOptions.BlockWrap;
            }

            try
            {
                if (Style == PromptStyle.AnyKey)
                {
                    _inputString = ReadKey(false);
                }
                else
                {
                    _inputString = ReadLine(seed);
                }

                if (_inputString == null)
                {
                    Status = PromptStatus.Escaped;
                }
                else
                if (Style == PromptStyle.Confirm && _inputString == YesValue)
                {
                    Status = PromptStatus.Yes;
                }
                else
                if (Style == PromptStyle.Confirm && _inputString == NoValue)
                {
                    Status = PromptStatus.No;
                }
                else
                {
                    Status = PromptStatus.Entered;
                }

                return Status;
            }
            finally
            {
                // Reset
                ScreenIO.ScrollBreak = scrollBreak;
                ScreenIO.IsCursorVisible = cursor;
            }
        }

        /// <summary>
        /// Calls <see cref="TryResult(Type, out object)"/> with the type of T and returns the
        /// result of the conversion on success. On failure, it throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Invalid result</exception>
        public T GetValue<T>()
            where T : struct, IConvertible
        {
            if (TryResult(out T value))
            {
                return value;
            }

            if (Status == PromptStatus.Escaped)
            {
                throw new InvalidOperationException($"User escaped (no value)");
            }

            throw new InvalidOperationException($"Not a valid {typeof(T).Name} value");
        }

        /// <summary>
        /// Try converts the <see cref="InputString"/> string to value of "type". The method returns
        /// false if <see cref="InputString"/> is null or on conversion failure. If <see cref="Style"/>
        /// is <see cref="PromptStyle.Confirm"/> and "type" is that of <see cref="bool"/>, the
        /// result will be true of false according <see cref="YesValue"/> and <see cref="NoValue"/>.
        /// </summary>
        public bool TryResult(Type type, out object? value)
        {
            return _history.TryConvert(type, InputString, out value);
        }

        /// <summary>
        /// Equivalent to TryResult(<see cref="ValueType"/>, out value)
        /// </summary>
        public bool TryResult(out object? value)
        {
            return _history.TryConvert(ValueType, InputString, out value);
        }

        /// <summary>
        /// Calls <see cref="TryResult(Type, out object)"/> with the type of T.
        /// </summary>
        public bool TryResult<T>(out T value)
            where T : struct, IConvertible
        {
            if (_history.TryConvert(typeof(T), InputString, out object? temp) && temp != null)
            {
                value = (T)temp;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Calls <see cref="TryResult(Type, out object)"/> with the type of T. Additionally,
        /// the result is true only if value is in the range [min, max].
        /// </summary>
        public bool TryResult<T>(T min, T max, out T value)
            where T : struct, IConvertible, IComparable
        {
            return TryResult(out value) && value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Protected access for unit testing only.
        /// </summary>
        protected static string GetPrefix(PromptStyle style, Type? type)
        {
            switch (style)
            {
            case PromptStyle.AnyKey:
                return "Press any key ... ";
            case PromptStyle.Text:
                return type == null ? "Input?: " : "Input? [%TYPE%]: ";
            case PromptStyle.HidePassword:
            case PromptStyle.ShowPassword:
                return "Password?: ";
            case PromptStyle.FileName:
                return "Filename?: ";
            case PromptStyle.FilePath:
                return "Path?: ";
            case PromptStyle.Confirm:
                return "Confirm? [%Y%/%N%]: ";
            default:
                return "";
            }
        }

        private static bool IsShortcut(ConsoleKey key)
        {
            return key == ConsoleKey.Home || key == ConsoleKey.End || key == ConsoleKey.Insert
                || key == ConsoleKey.PageUp || key == ConsoleKey.PageDown;
        }

        private static ConsoleKeyInfo[] ReadInputBuffer()
        {
            // Read keys in one quick pass,
            // others does not pick up paste operations
            var list = new List<ConsoleKeyInfo>
            {
                Console.ReadKey(true)
            };

            while (Console.KeyAvailable)
            {
                list.Add(Console.ReadKey(true));
            }

            // Prevent holding down of key repeating forever (Linux is slow to respond)
            int n = 0;
            int count = 0;
            ConsoleKey last = (ConsoleKey)(-1);

            while (n < list.Count)
            {
                if (list[n].Key == last)
                {
                    count += 1;

                    if (count > 10)
                    {
                        list.RemoveAt(n);
                        continue;
                    }
                }
                else
                {
                    count = 0;
                    last = list[n].Key;
                }

                n += 1;
            }


            return list.ToArray();
        }

        private string? ReadKey(bool showKey)
        {
            ScreenIO.IsCursorVisible = showKey;
            int pfxLen = PrintPrefix(Prefix, null, 1, 1);

            var rk = Console.ReadKey(true);
            var key = rk.Key;
            char kchar = rk.KeyChar;

            if (showKey)
            {
                if (kchar >= 0x20)
                {
                    ScreenIO.Print(kchar.ToString(), ColorId.Input, _format);
                }

                ScreenIO.PrintLn();
            }
            else
            {
                // Erase prompt
                ScreenIO.PosXY = PosXY;
                ScreenIO.Print(new string(' ', pfxLen), ColorId.Text, _format);
                ScreenIO.PosXY = PosXY;
            }

            if (key == ConsoleKey.Escape)
            {
                return null;
            }

            if (kchar == 0)
            {
                return key.ToString();
            }

            return kchar.ToString();
        }

        private string? ReadLine(string? seed)
        {
            ScreenIO.IsCursorVisible = true;
            PrintPrefix(Prefix, seed, MinLength, MaxLength);

            int maxLen = MaxLength;
            var buffer = new StringBuilder(seed);
            bool shortcuts = ShortCuts && Style == PromptStyle.Text;

            // Input loop
            while (true)
            {
                var keyInfo = ReadInputBuffer();

                for (int n = 0; n < keyInfo.Length; ++n)
                {
                    var key = keyInfo[n].Key;
                    char kchar = keyInfo[n].KeyChar;

                    if (key == ConsoleKey.Escape)
                    {
                        // Always allow escape
                        ScreenIO.PrintLn();
                        return null;
                    }
                    else
                    if (key == ConsoleKey.Enter)
                    {
                        var buf = buffer.ToString();
                        var rslt = _history.AcceptCommit(buf);

                        if (rslt != null)
                        {
                            ScreenIO.PrintLn();
                            return rslt;
                        }

                        FlashError(buf);
                    }
                    else
                    if (key == ConsoleKey.Backspace)
                    {
                        if (buffer.Length > 0)
                        {
                            // This is about the best we can do which wraps reliably. It's great on
                            // Windows, but is slow on Linux (at time of writing). Attempts to
                            // improve things by erasing only a single screen character were worse.
                            // Also, on Linux, we cannot hide the cursor.
                            ScreenIO.IsCursorVisible = false;

                            // Overwrite entire prompt
                            ScreenIO.PosXY = _inputXY;
                            ScreenIO.Print(new string(' ', buffer.Length), ColorId.Input, _format);

                            // Buffer back
                            buffer.Length -= 1;

                            // Re-write
                            ScreenIO.PosXY = _inputXY;

                            if (Style == PromptStyle.HidePassword)
                            {
                                // Don't reprint clear text
                                ScreenIO.Print(new string('*', buffer.Length), ColorId.Input, _format);
                            }
                            else
                            {
                                ScreenIO.Print(buffer.ToString(), ColorId.Input, _format);
                            }

                            ScreenIO.IsCursorVisible = true;
                        }
                    }
                    else
                    if (key == ConsoleKey.Delete)
                    {
                        // Delete all input
                        ScreenIO.PosXY = _inputXY;
                        ScreenIO.Print(new string(' ', buffer.Length), ColorId.Input, _format);
                        ScreenIO.PosXY = _inputXY;

                        buffer.Clear();
                    }
                    else
                    if (shortcuts && IsShortcut(key))
                    {
                        // Shortcuts allowed only at input start.
                        var temp = key.ToString();

                        if (buffer.Length == 0 && (maxLen < 0 || maxLen >= temp.Length))
                        {
                            buffer.Append(temp);
                            ScreenIO.Print(temp, ColorId.Input, _format);
                        }
                    }
                    else
                    if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
                    {
                        var buf = key == ConsoleKey.UpArrow ? _history.BackHistory() : _history.ForwardHistory();

                        if (buf != null)
                        {
                            if (buf.Length > maxLen)
                            {
                                buf = buf.Substring(0, maxLen);
                            }

                            ScreenIO.PosXY = _inputXY;

                            if (buffer.Length > buf.Length)
                            {
                                ScreenIO.Print(new string(' ', buffer.Length), ColorId.Input, _format);
                                ScreenIO.PosXY = _inputXY;
                            }

                            ScreenIO.Print(buf, ColorId.Input, _format);

                            buffer.Clear();
                            buffer.Append(buf);
                        }
                    }
                    else
                    if (buffer.Length < maxLen && kchar >= 0x20)
                    {
                        buffer.Append(kchar);

                        if (Style == PromptStyle.HidePassword)
                        {
                            ScreenIO.Print("*", ColorId.Input, _format);
                        }
                        else
                        {
                            ScreenIO.Print(kchar.ToString(), ColorId.Input, _format);
                        }
                    }
                }
            }
        }

        private int PrintPrefix(string? pfx, string? seed, int minLen, int maxLen)
        {
            if (!string.IsNullOrEmpty(pfx))
            {
                pfx = pfx.Replace("%MINLEN%", minLen.ToString());
                pfx = pfx.Replace("%MAXLEN%", maxLen.ToString());
                pfx = pfx.Replace("%Y%", YesValue);
                pfx = pfx.Replace("%N%", NoValue);
                pfx = pfx.Replace("%TYPE%", ValueType.Name);

                ScreenIO.Print(pfx, Color, _format);
            }

            _inputXY = ScreenIO.PosXY;
            ScreenIO.Print(seed, ColorId.Input);

            return pfx?.Length ?? 0;
        }

        private void FlashError(string? str)
        {
            // Timer prevents buffering of repeated errors
            if (!string.IsNullOrEmpty(str) && (_flashWatch == null || _flashWatch.ElapsedMilliseconds > 2 * FlashDelay))
            {
                // Flash read
                ScreenIO.PosXY = _inputXY;
                _flashWatch = Stopwatch.StartNew();

                ScreenIO.Print(str, ColorId.Critical, _format);

                Thread.Sleep(FlashDelay);

                ScreenIO.PosXY = _inputXY;
                ScreenIO.Print(str, ColorId.Input, _format);
            }
        }

    }

}
