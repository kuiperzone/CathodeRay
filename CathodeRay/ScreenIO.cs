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
using System.Runtime.InteropServices;
using System.Text;

namespace KuiperZone.CathodeRay
{
    /// <summary>
    /// The static class <see cref="ScreenIO"/> is an abstraction wrapper around the <see cref="Console"/> class,
    /// providing a subset of routines to format output text, handling color themes and providing scroll-break
    /// interaction. It is important to use this class consistently (rather than Console) within
    /// <see cref="CathodeRayPage"/> classes as it keeps track of wrapping and line positions.
    /// </summary>
    public static class ScreenIO
    {
        /// <summary>
        /// Default width in characters.
        /// </summary>
        public const int DefaultWidth = 100;

        private const int RecursionDepth = 100;
        private const string AbortMessage = "[Cancelled]";
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // Properties
        private static Theme _theme = new();
        private static bool _transparentBackground;
        private static int _tabSize = 4;

        // Internal
        private static bool _isCursorVisible = true;
        private static bool _firstPrint = true;
        private static bool _scrollEscaped;
        private static int _scrollCount;
        private static readonly Stopwatch _escapeWatch = Stopwatch.StartNew();

        // Cache console values. We do these to minimize calls on
        // Console class, especially as Linux terminal is slow.
        // Initial values are dummy (may get used in unit testing)
        private static int _startXCache = 0;
        private static int _printWidthCache = DefaultWidth;
        private static int _windowWidthCache = DefaultWidth;
        private static int _windowHeightCache = 50;

        /// <summary>
        /// Gets or sets the <see cref="CathodeRay.Theme"/> instance. Setting this to null will reset
        /// a new default instance of <see cref="CathodeRay.Theme"/>.
        /// </summary>
        public static Theme Theme
        {
            get { return _theme; }
            set { _theme = value ?? new Theme(); }
        }

        /// <summary>
        /// Gets whether or not to print the screen background color. If true, the background is not printed.
        /// </summary>
        public static bool TransparentBackground
        {
            get { return _transparentBackground; }

            set
            {
                if (_transparentBackground != value)
                {
                    _transparentBackground = value;
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Gets or sets the base <see cref="ScreenOptions"/> value for use with print calls. For
        /// alignment flags, these define the horizontal position of the entire page as defined by
        /// <see cref="ActualWidth"/>. Other flags can be overridden by specifying a formatting
        /// value with the print statement. The default is value is <see cref="ScreenOptions.WordWrap"/>.
        /// </summary>
        public static ScreenOptions Options { get; set; } = ScreenOptions.WordWrap;

        /// <summary>
        /// Gets or sets the number of tab characters in the range [2, 16]. The default is 4.
        /// </summary>
        public static int TabSize
        {
            get { return _tabSize; }
            set { _tabSize = Math.Clamp(value, 2, 16); }
        }

        /// <summary>
        /// The CultureInfo provider to use when converting arguments to string and when
        /// transforming case. The default is CurrentCulture.
        /// </summary>
        public static CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the current cursor X position. Returns 0 if the console IO handle is invalid.
        /// </summary>
        public static int PosX
        {
            get
            {
                try
                {
                    return Console.CursorLeft;
                }
                catch
                {
                    // Catch for unit test only (no handle)
                    return 0;
                }
            }

            set
            {
                try
                {
                    Console.CursorLeft = value;
                }
                catch
                {
                    // Catch for unit test only (no handle)
                }
            }
        }

        /// <summary>
        /// Gets or sets the current cursor Y position. Returns 0 if the console IO handle is invalid.
        /// </summary>
        public static int PosY
        {
            get
            {
                try
                {
                    return Console.CursorTop;
                }
                catch
                {
                    // Catch for unit test only (no handle)
                    return 0;
                }
            }

            set
            {
                try
                {
                    Console.CursorTop = value;
                }
                catch
                {
                    // Catch for unit test only (no handle)
                }
            }
        }

        /// <summary>
        /// Gets or sets the current cursor X,Y position as a Tuple. The result is a new instance on
        /// each call. Returns (0, 0) if the console IO handle is invalid.
        /// </summary>
        public static Tuple<int, int> PosXY
        {
            get
            {
                try
                {
                    return Tuple.Create(Console.CursorLeft, Console.CursorTop);
                }
                catch
                {
                    // Catch for unit test only (no handle)
                    return Tuple.Create(0, 0);
                }
            }

            set
            {
                try
                {
                    Console.CursorLeft = value.Item1;
                    Console.CursorTop = value.Item2;
                }
                catch
                {
                    // Catch for unit test only (no handle)
                }
            }
        }

        /// <summary>
        /// Gets the printing starting left position after a new line. The <see cref="PosX"/>
        /// value will be equal to this after a new line and the value is non-zero if <see
        /// cref="Options"/> specifies center or right page alignment.
        /// </summary>
        public static int StartPosX
        {
            get
            {
                var align = Options.Alignment();

                if (align != ScreenOptions.None)
                {
                    int sw = TerminalWidth - 1;
                    int pw = FormatWidth > 10 ? Math.Min(FormatWidth, sw) : sw;

                    if (align == ScreenOptions.Center)
                    {
                        return Math.Max((sw - pw) / 2, 0);
                    }
                    else
                    if (align == ScreenOptions.Right)
                    {
                        return Math.Max(sw - pw, 0);
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed value of <see cref="ActualWidth"/>. The initial value is
        /// <see cref="DefaultWidth"/>.
        /// </summary>
        public static int FormatWidth { get; set; } = DefaultWidth;

        /// <summary>
        /// On Windows, gets Console.WindowWidth. On other systems, always returns <see cref="DefaultWidth"/>.
        /// </summary>
        public static int TerminalWidth
        {
            get { return IsWindows ? Console.WindowWidth : DefaultWidth; }
        }

        /// <summary>
        /// Gets the output width allowed for formatted print alignment and wrapping purposes. If <see
        /// cref="FormatWidth"/> is greater than 10, the result here is the minimum of <see
        /// cref="FormatWidth"/> and <see cref="TerminalWidth"/> - 1. If <see cref="FormatWidth"/> is 0,
        /// the result is equal to <see cref="TerminalWidth"/> - 1. Note that wrapping is not enforced
        /// unless output is printed with a wrapping formatting flag.
        /// </summary>
        public static int ActualWidth
        {
            get
            {
                int sw = TerminalWidth - 1;
                return FormatWidth > 10 ? Math.Min(FormatWidth, sw) : sw;
            }
        }

        /// <summary>
        /// Gets or sets whether Console.Cursor is visible on Windows. On other systems, Console.Cursor is not
        /// supported, and the value here is a dummy which has no affect on Console.
        /// </summary>
        public static bool IsCursorVisible
        {
            get { return IsWindows ? Console.CursorVisible : _isCursorVisible; }

            set
            {
                if (IsWindows) Console.CursorVisible = value;
                else _isCursorVisible = value;
            }
        }

        /// <summary>
        /// Gets whether scroll prompting is active. When true and print output exceeds the vertical
        /// screen space, the user will be prompted with an option to "scroll more". Default is false.
        /// </summary>
        public static bool ScrollBreak { get; set; }

        /// <summary>
        /// Gets or sets or a percentage value displayed along with the scroll message to indicate
        /// progress. The initial value is -1, which implies that this information is not shown. If
        /// the total length of a print sequence is known, this property can optionally be set to
        /// value in the range [0, 100]. This value is reset by <see cref="Reset"/> and is ignored
        /// if less than 0.
        /// </summary>
        public static double ScrollProgress { get; set; } = -1;

        /// <summary>
        /// Gets whether the user has escaped the current print operation. It is set to false
        /// by call to <see cref="Reset"/>.
        /// </summary>
        public static bool IsPrintCancelled { get; private set; }

        /// <summary>
        /// The total number of lines printed since the last call to <see cref="Cls"/>. This takes
        /// into consideration strings that were split for wrapping.
        /// </summary>
        public static long LineCount { get; private set; }

        /// <summary>
        /// Static utility which truncates the string if it exceeds length characters according
        /// to the <see cref="Truncation"/> specified.
        /// </summary>
        public static string? Truncate(string? s, int length, Truncation style = Truncation.Simple)
        {
            if (s != null && s.Length > length)
            {
                if (style == Truncation.EllipsesCenter && length > 4)
                {
                    // 01234567   -> 0...7 [5]
                    // 01234567   -> 01...7 [6]

                    // 012345678  -> 01...78 [7]
                    // 0123456789 -> 01...89 [7]
                    // 0123456789 -> 012...89 [8]
                    // 0123456789 -> 012...789 [9]
                    int x0 = length / 2 - 1;
                    int x1 = s.Length - x0 + (length % 2 == 0 ? 1 : 0);
                    return s.Substring(0, x0) + "..." + s.Substring(x1);
                }
                if (style != Truncation.Simple && length > 3)
                {
                    // 0123  -> 0...
                    return s.Substring(0, length - 3) + "...";
                }
                else
                {
                    return s.Substring(0, length);
                }
            }

            return s;
        }

        /// <summary>
        /// Static utility which inserts a space between each character to give double spaced text.
        /// </summary>
        public static string? DoubleSpace(string? text)
        {
            if (text == null)
            {
                return null;
            }

            // "ADAPA" -> "A D A P A"
            // "ADA\nPA" -> "A D A\nP A"
            int len = text.Length;
            var sb = new StringBuilder();

            bool ld = false;

            for (int n = 0; n < len; ++n)
            {
                var c = text[n];

                if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || c == ' ' || c == '\u00A0')
                {
                    // Non-break space
                    if (ld && c == '\u00A0') sb.Append('\u00A0');
                    else if (ld) sb.Append(' ');

                    ld = true;
                }
                else
                {
                    ld = false;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Static utility which splits text on new line characters. If text is null, the result is
        /// an empty array.
        /// </summary>
        public static string[] SplitLines(string? text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // FF(0C), PARA-SEP(2029) then LF(0A), NEL(85), LINE-SEP(2028)
                // https://www.unicode.org/standard/reports/tr13/tr13-5.html
                // return text.Split('\x0A', '\x0C', '\u0085', '\u2028', '\u2029');
                var pages = text.Split('\x0C', '\u2029');

                if (pages.Length > 1)
                {
                    var temp = new List<string>(Math.Min(text.Length / 10, 16));

                    for (int n = 0; n < pages.Length; ++n)
                    {
                        temp.AddRange(SplitLines(pages[n]));

                        if (n < pages.Length - 1)
                        {
                            temp.Add("");
                        }
                    }

                    return temp.ToArray();
                }

                var lines = text.Split('\n', '\u0085', '\u2028');

                for (int n = 0; n < lines.Length; ++n)
                {
                    lines[n] = lines[n].TrimEnd('\r');
                }

                return lines;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Clears the Console and calls <see cref="Reset"/>.
        /// </summary>
        public static void Cls()
        {
            try
            {
                // Should we leave the background alone?
                // Some Linux flavors have pastel backgrounds.
                if (!TransparentBackground)
                {
                    Console.BackgroundColor = _theme[ColorId.Background];
                }

                Console.Clear();
                Console.CursorLeft = StartPosX;
            }
            catch
            {
            }

            LineCount = 0;
            _firstPrint = false;

            Reset();
        }

        /// <summary>
        /// Resets <see cref="IsPrintCancelled"/> and the scrolling break prompt without clearing the
        /// screen.
        /// </summary>
        public static void Reset()
        {
            _scrollCount = 0;
            _scrollEscaped = false;
            IsPrintCancelled = false;
        }

        /// <summary>
        /// Equivalent to: <see cref="Print(string?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void Print(string? text)
        {
            PrintInternal(text, ColorId.Text, ScreenOptions.None, false);
        }

        /// <summary>
        /// Equivalent to: <see cref="Print(string?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is supplied.
        /// </summary>
        public static void Print(string? text, ScreenOptions pf)
        {
            PrintInternal(text, ColorId.Text, pf, false);
        }

        /// <summary>
        /// Equivalent to: <see cref="Print(string?, ColorId, ScreenOptions)"/>, where the
        /// color is supplied and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void Print(string? text, ColorId col)
        {
            PrintInternal(text, col, ScreenOptions.None, false);
        }

        /// <summary>
        /// The <see cref="ColorId"/> is mapped to ConsoleColor using <see cref="Theme"/>.
        /// Otherwise the method is equivalent to:
        /// <see cref="Print(string?, ConsoleColor, ScreenOptions)"/>
        /// </summary>
        public static void Print(string? text, ColorId col, ScreenOptions pf)
        {
            PrintInternal(text, col, pf, false);
        }

        /// <summary>
        /// Prints the text string according to color and the <see cref="ScreenOptions"/> value. This
        /// variant of Print() uses ConsoleColor rather than a <see cref="ColorId"/>
        /// value. This prints text of any color, independent of the <see cref="Theme"/>.
        /// </summary>
        public static void Print(string? text, ConsoleColor col, ScreenOptions pf)
        {
            PrintInternal(text, col, pf, false);
        }

        /// <summary>
        /// Equivalent to: <see cref="Print(object?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void Print(object? obj)
        {
            PrintInternal(FormatObj(obj), ColorId.Text, ScreenOptions.None, false);
        }

        /// <summary>
        /// Equivalent to: <see cref="Print(object?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is supplied.
        /// </summary>
        public static void Print(object? obj, ScreenOptions pf)
        {
            PrintInternal(FormatObj(obj), ColorId.Text, pf, false);
        }

        /// <summary>
        /// Equivalent to: <see cref="Print(object?, ColorId, ScreenOptions)"/>, where the
        /// color is supplied and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void Print(object? obj, ColorId col)
        {
            PrintInternal(FormatObj(obj), col, ScreenOptions.None, false);
        }

        /// <summary>
        /// The <see cref="ColorId"/> is mapped to ConsoleColor using <see cref="Theme"/>.
        /// Otherwise the method is equivalent to:
        /// <see cref="Print(object?, ConsoleColor, ScreenOptions)"/>
        /// </summary>
        public static void Print(object? obj, ColorId col, ScreenOptions pf)
        {
            PrintInternal(FormatObj(obj), col, pf, false);
        }

        /// <summary>
        /// Converts the object to string using <see cref="Culture"/> (where applicable) and prints
        /// the text according to color and the <see cref="ScreenOptions"/> value. This variant of
        /// Print() uses ConsoleColor rather than a <see cref="ColorId"/> value. This prints
        /// text of any color, independent of the <see cref="Theme"/>.
        /// </summary>
        public static void Print(object? obj, ConsoleColor col, ScreenOptions pf)
        {
            PrintInternal(FormatObj(obj), col, pf, false);
        }

        /// <summary>
        /// Prints a new line.
        /// </summary>
        public static void PrintLn()
        {
            PrintInternal(null, ColorId.Text, ScreenOptions.None, true);
        }

        /// <summary>
        /// Equivalent to: <see cref="PrintLn(string?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void PrintLn(string? text)
        {
            PrintInternal(text, ColorId.Text, ScreenOptions.None, true);
        }

        /// <summary>
        /// Equivalent to: <see cref="PrintLn(string?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is supplied.
        /// </summary>
        public static void PrintLn(string? text, ScreenOptions pf)
        {
            PrintInternal(text, ColorId.Text, pf, true);
        }

        /// <summary>
        /// Equivalent to: <see cref="PrintLn(string?, ColorId, ScreenOptions)"/>, where the
        /// color is supplied and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void PrintLn(string? text, ColorId col)
        {
            PrintInternal(text, col, ScreenOptions.None, true);
        }

        /// <summary>
        /// The <see cref="ColorId"/> is mapped to ConsoleColor using <see cref="Theme"/>.
        /// Otherwise the method is equivalent to:
        /// <see cref="PrintLn(string?, ConsoleColor, ScreenOptions)"/>
        /// </summary>
        public static void PrintLn(string? text, ColorId col, ScreenOptions pf)
        {
            PrintInternal(text, col, pf, true);
        }

        /// <summary>
        /// Prints the text string according to color and the <see cref="ScreenOptions"/> value. This
        /// variant of PrintLn() uses ConsoleColor rather than a <see cref="ColorId"/>
        /// value. This prints text of any color, independent of the <see cref="Theme"/>. It is
        /// followed by a newline.
        /// </summary>
        public static void PrintLn(string? text, ConsoleColor col, ScreenOptions pf)
        {
            PrintInternal(text, col, pf, true);
        }

        /// <summary>
        /// Equivalent to: <see cref="PrintLn(object?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void PrintLn(object? obj)
        {
            PrintInternal(FormatObj(obj), ColorId.Text, ScreenOptions.None, true);
        }

        /// <summary>
        /// Equivalent to: <see cref="PrintLn(object?, ColorId, ScreenOptions)"/>, where the
        /// color is <see cref="ColorId.Text"/> and <see cref="ScreenOptions"/> is supplied.
        /// </summary>
        public static void PrintLn(object? obj, ScreenOptions pf)
        {
            PrintInternal(FormatObj(obj), ColorId.Text, pf, true);
        }

        /// <summary>
        /// Equivalent to: <see cref="PrintLn(object?, ColorId, ScreenOptions)"/>, where the
        /// color is supplied and <see cref="ScreenOptions"/> is None.
        /// </summary>
        public static void PrintLn(object? obj, ColorId col)
        {
            PrintInternal(FormatObj(obj), col, ScreenOptions.None, true);
        }

        /// <summary>
        /// The <see cref="ColorId"/> is mapped to ConsoleColor using <see cref="Theme"/>.
        /// Otherwise the method is equivalent to:
        /// <see cref="PrintLn(object?, ConsoleColor, ScreenOptions)"/>
        /// </summary>
        public static void PrintLn(object? obj, ColorId col, ScreenOptions pf)
        {
            PrintInternal(FormatObj(obj), col, pf, true);
        }

        /// <summary>
        /// Converts the object to string using <see cref="Culture"/> (where applicable) and prints
        /// the text according to color and the <see cref="ScreenOptions"/> value. This variant of
        /// PrintLn() uses ConsoleColor rather than a <see cref="ColorId"/> value. This prints
        /// text of any color, independent of the <see cref="Theme"/>. It is followed by a newline.
        /// </summary>
        public static void PrintLn(object? obj, ConsoleColor col, ScreenOptions pf)
        {
            PrintInternal(FormatObj(obj), col, pf, true);
        }

        /// <summary>
        /// Prints a message and then sleeps for "delay" milliseconds. Convenience when printing a
        /// short message, such as "OK", followed by a screen clear. It does nothing if "text" is
        /// empty or null, or delay is negative.
        /// </summary>
        public static void PrintPause(string? text, ColorId col, int delay = 1000)
        {
            if (!string.IsNullOrEmpty(text) && delay > -1)
            {
                Print(text, col);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Prints exception. If showStack is true, the stack is shown and the output is suitable
        /// for diagnostic purposes. If showStack is false, only the error message is shown.
        /// </summary>
        public static void PrintException(Exception e, bool showStack)
        {
            if (showStack)
            {
                PrintLn();
                PrintLn("-------- EXCEPTION --------", ColorId.Critical);

                PrintLn(e.Message, ColorId.Warning);
                PrintLn();
                PrintLn(e.StackTrace);
                PrintLn();
            }
            else
            {
                PrintLn(e.Message, ColorId.Warning);
            }
        }

        private static ScreenOptions MergeFormatting(ScreenOptions flagBase, ScreenOptions flags)
        {
            // Merges flags according to grouping with those of flags taking precedence over those of
            // base. We don't include alignment, as Formatting alignment applies to entire page.
            int result = (int)flags;
            int ibase = (int)flagBase;

            const int CaseMask = (int)(ScreenOptions.NoCase | ScreenOptions.Lowercase | ScreenOptions.Uppercase);

            if ((result & CaseMask) == 0)
            {
                result |= ibase & CaseMask;
            }

            const int WrapMask = (int)(ScreenOptions.NoWrap | ScreenOptions.WordWrap | ScreenOptions.BlockWrap);

            if ((result & WrapMask) == 0)
            {
                result |= ibase & WrapMask;
            }

            return (ScreenOptions)result;
        }

        /// <summary>
        /// Protected only for unit testing (otherwise treat as private). Substitutes tabs and break
        /// line at first juncture. Returns null if text not modified, or either an array of length
        /// 1 or length 2 with item at index 1 containing remaining unprocessed text.
        /// </summary>
        private static string[]? WrapTabLine(string? text, int left, ScreenOptions fmt, int width)
        {
            if (!string.IsNullOrEmpty(text))
            {
                left = Math.Max(left - _startXCache, 0);

                int spcPos = 0;
                int altPos = 0;
                int length = text.Length;
                int tabSz = TabSize;
                bool wordWrap = fmt.Wrapping() == ScreenOptions.WordWrap;

                int pos = -1;
                bool modified = false;

                while (++pos < length)
                {
                    char c = text[pos];

                    if (c == '\t')
                    {
                        // Handle tabs
                        int rem = tabSz - ((left + pos) % tabSz);
                        text = text.Substring(0, pos) + new string(' ', rem) + text.Substring(pos + 1);

                        c = ' ';
                        modified = true;
                        length = text.Length;
                    }

                    if (wordWrap)
                    {
                        if (c <= ' ')
                        {
                            // Primary break
                            spcPos = pos;
                        }
                        else
                        if (c == '-' || c == ':' || c == '=' || c == '.' || c == '/' || c == '\\'
                            || c == '~' || c == ')' || c == '|' || c == '}' || c == ']')
                        {
                            if (pos < width)
                            {
                                // Secondary break (prettier)
                                altPos = pos + 1;
                            }
                        }
                    }

                    if (pos == width)
                    {
                        if (spcPos != 0)
                        {
                            pos = spcPos;
                        }
                        else
                        if (altPos != 0)
                        {
                            pos = altPos;
                        }

                        // Split lines at first break
                        var split = new string[2];
                        split[0] = text.Substring(0, pos);
                        split[1] = text.Substring(pos);

                        if (wordWrap)
                        {
                            split[0] = split[0].TrimEnd();
                            split[1] = split[1].TrimStart();
                        }

                        return split;
                    }
                }

                if (modified)
                {
                    return new string[1] { text };
                }
            }

            // Not modified
            return null;
        }

        private static string? FormatObj(object? arg)
        {
            // Format to Culture if arg is IFormattable.

            if (arg != null)
            {
                if (arg is IFormattable formattable)
                {
                    return formattable.ToString(null, Culture);
                }

                return arg.ToString();
            }

            return null;
        }

        private static void SetForegroundColor(ConsoleColor col)
        {
            try
            {
                Console.ForegroundColor = col;
            }
            catch
            {
            }
        }

        private static void PrintInternal(string? text, ColorId col, ScreenOptions fmt, bool feedEnd)
        {
            // All Print() and PrintLn() method should call PrintInternal variant.
            PrintInternal(text, _theme[col], fmt, feedEnd);
        }

        private static void PrintInternal(string? text, ConsoleColor col, ScreenOptions fmt, bool feedEnd)
        {
            // All Print() and PrintLn() method should call PrintInternal variant.
            if (!IsPrintCancelled)
            {
                bool cursorVisible = IsCursorVisible;

                try
                {
                    // Cache internal values so we don't have to keep reading them
                    _startXCache = StartPosX;
                    _printWidthCache = ActualWidth;

                    // Will throw if no handle
                    _windowWidthCache = Console.WindowWidth;
                    _windowHeightCache = Console.WindowHeight;

                    // Cursor off if we can
                    IsCursorVisible = false;

                    // Set text color
                    SetForegroundColor(col);

                    if (!string.IsNullOrEmpty(text))
                    {
                        if (_firstPrint)
                        {
                            if (PosX > _startXCache)
                            {
                                ConsoleWriteLine(false);
                            }
                            else
                            {
                                PosX = _startXCache;
                            }

                            _firstPrint = false;
                        }

                        PrintLines(SplitLines(text), MergeFormatting(Options, fmt));
                    }

                    if (feedEnd)
                    {
                        ConsoleWriteLine();
                    }
                }
                catch
                {
                }

                // Reset
                SetForegroundColor(Theme.SystemForeground);
                IsCursorVisible = cursorVisible;
            }
        }

        private static void PrintLines(string[] lines, ScreenOptions fmt)
        {
            for (int n = 0; n < lines.Length; ++n)
            {
                string line = lines[n];
                var cs = fmt.TextCase();

                if (cs == ScreenOptions.Uppercase)
                {
                    line = line.ToUpper(Culture);
                }
                else
                if (cs == ScreenOptions.Lowercase)
                {
                    line = line.ToLower(Culture);
                }

                PrintFragment(line, fmt);

                if (n < lines.Length - 1)
                {
                    ConsoleWriteLine();
                }
            }
        }

        private static void PrintFragment(string line, ScreenOptions fmt, int recursion = 0)
        {
            if (!string.IsNullOrEmpty(line))
            {
                int posX = PosX;
                int left = posX;

                int wrapWidth = _windowWidthCache;
                int remainWidth = wrapWidth - left;

                // WRAPPING
                if (fmt.Wrapping() != ScreenOptions.None)
                {
                    // Remaining on the current line
                    wrapWidth = _printWidthCache;
                    remainWidth = _startXCache + wrapWidth - left;
                }

                var multi = WrapTabLine(line, left, fmt, remainWidth);

                // Put a limit on the recursion
                if (multi != null)
                {
                    // If length is 1, it means we have modified the line, but not broken on lines.
                    line = multi[0];

                    if (multi.Length > 1 && recursion < RecursionDepth)
                    {
                        // Broken on width. We use recursion.
                        for (int n = 0; n < multi.Length; ++n)
                        {
                            PrintFragment(multi[n], fmt, ++recursion);

                            if (n < multi.Length - 1)
                            {
                                ConsoleWriteLine();
                            }
                        }

                        // Done
                        return;
                    }
                }

                // ALIGNMENT
                if (fmt.HasFlag(ScreenOptions.Center))
                {
                    left += Math.Max(remainWidth - line.Length, 0) / 2;
                }
                else
                if (fmt.HasFlag(ScreenOptions.Right))
                {
                    left += Math.Max(remainWidth - line.Length, 0);
                }

                /*
                // Assuming not printed if here. Otherwise we need new cursorLeft value.
                if (left > posX)
                {
                    PosX = Math.Min(left, BufferWidth);
                }

                ConsoleWrite(line);
                */

                // Alternative to above which has two less Console calls (note that Console is slow
                // on Linux). It also means that we could expose the string to unit testing and look
                // for left padding characters.
                ConsoleWrite(line.PadLeft(Math.Max(line.Length + left - posX, 0)));

                if (wrapWidth > 0)
                {
                    // Left addition assumes we have not left padded line
                    int delta = (line.Length + left - _startXCache - 1) / wrapWidth;

                    LineCount += delta;
                    _scrollCount += delta;
                }
            }
        }

        private static void ConsoleWrite(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    Console.Write(s);
                }
                catch
                {
                }
            }
        }

        private static void ConsoleWriteLine(bool handleScroll = true)
        {
            if (!IsPrintCancelled)
            {
                LineCount += 1;
                _scrollCount += 1;

                try
                {
                    Console.WriteLine();
                    Console.CursorLeft = _startXCache;

                    if (handleScroll)
                    {
                        HandleScrollBreak();
                    }
                }
                catch
                {
                }
            }
        }

        private static void CancelPrinting()
        {
            SetForegroundColor(Theme[ColorId.Warning]);

            ConsoleWriteLine(false);
            ConsoleWrite(AbortMessage);
            ConsoleWriteLine(false);

            _scrollEscaped = true;
            _scrollCount = 0;
            IsPrintCancelled = true;

            // Expect to be caught
            throw new InvalidOperationException("Cancelled");
        }

        private static bool IsEscapePressed()
        {
            if (_escapeWatch.ElapsedMilliseconds > 250)
            {
                try
                {
                    _escapeWatch.Restart();
                    return Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape;
                }
                catch
                {
                }
            }

            return false;
        }

        private static void HandleScrollBreak()
        {
            if (!IsPrintCancelled)
            {
                // Detect escape key on lengthy printouts
                if (IsEscapePressed())
                {
                    CancelPrinting();
                }
                else
                if (!_scrollEscaped && ScrollBreak && _scrollCount >= _windowHeightCache - 1 && _windowHeightCache > 5)
                {
                    // Don't attempt to scroll if window height appears tiny.

                    // Hold initial state.
                    var initPos = PosXY;
                    int scrollCount = _scrollCount;
                    ScreenOptions format = Options;

                    try
                    {
                        if (initPos.Item1 > _startXCache)
                        {
                            // Don't place the prompt in the middle of text
                            ConsoleWriteLine(false);
                        }

                        // Force to left
                        PosX = _startXCache;
                        Options |= ScreenOptions.NoWrap;

                        // Prevent prompter recursion
                        _scrollEscaped = true;

                        var prompt = new Prompter(PromptStyle.AnyKey);
                        prompt.Color = ColorId.Warning;

                        // Build message
                        prompt.Prefix = "More? [SPACE to continue, ENTER=all, ESC=quit, *+=1]";

                        if (ScrollProgress >= 0)
                        {
                            prompt.Prefix += " : " + ScrollProgress.ToString("0.0") + "%";
                        }

                        var rslt = prompt.Execute();
                        var key = prompt.InputString?.ToUpperInvariant();

                        if (rslt == PromptStatus.Escaped || key == "Q")
                        {
                            // Quit - will throw
                            CancelPrinting();
                        }
                        else
                        if (key == " " || key == "PAGEDOWN")
                        {
                            // Scroll to next stop
                            _scrollCount = 0;
                            _scrollEscaped = false;
                        }
                        else
                        if (key == "\n" || key == "\r" || key == "END")
                        {
                            // Scroll all
                            _scrollCount = 0;
                            _scrollEscaped = true;
                        }
                        else
                        {
                            // Scroll single line
                            _scrollCount = scrollCount - 1;
                            _scrollEscaped = false;
                        }

                        // Reset
                        PosXY = initPos;
                    }
                    finally
                    {
                        Options = format;
                    }
                }

            }

        }

    }
}
