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

using System.Text;

using KuiperZone.CathodeRay.Internal;

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// Displays text or binary data from the assigned <see cref="Source"/> stream. If a Stream
    /// instance is assigned to <see cref="Source"/>, the caller must dispose of it.
    /// </summary>
    public class FileContentPage : CathodeRayPage
    {
        private const string LineNumOpt = "L";
        private const int MaxLineCount = 999999;

        private const int AsciiSpacer = 2;
        private const int BlockSize = 16;
        private const int CharsPerByte = 3;
        private const int BinaryMarginWidth = 8 + 1;

        private bool _scrollBreak;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileContentPage(CathodeRayPage parent, string? title = "File Content")
            : base(parent, title)
        {
        }

        /// <summary>
        /// Gets or sets a seekable Stream instance to use when printing content.
        /// Important: the caller must dispose of the Stream instance.
        /// </summary>
        public Stream? Source { get; set; }

        /// <summary>
        /// Gets or sets an Encoding instance to use when reading <see cref="Source"/>. If the value
        /// is null, the file encoding will be determined automatically and setting merely forces
        /// the class to use a known encoding.
        /// </summary>
        public Encoding? SourceEncoding { get; set; }

        /// <summary>
        /// Gets or sets whether the content is to be treated as binary data. This will be determined
        /// automatically if <see cref="SourceEncoding"/> is null. However, setting this to true
        /// will force the class to display in binary view mode.
        /// </summary>
        public bool BinaryContent { get; set; }

        /// <summary>
        /// Gets or sets whether line numbers are shown. Default is true.
        /// </summary>
        public bool ShowLineNumbers { get; set; } = true;

        /// <summary>
        /// Overload for <see cref="CathodeRayPage.Execute"/> with supplied filename. The filename must
        /// exist. The <see cref="Source"/> value will be left unchanged on return.
        /// </summary>
        public void Execute(string filename)
        {
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var src = Source;

                try
                {
                    Source = file;
                    Execute();
                }
                finally
                {
                    Source = src;
                }
            }
        }

        /// <summary>
        /// Overrides <see cref="CathodeRayPage.OnExecutionStarted"/>.
        /// </summary>
        protected override void OnExecutionStarted()
        {
            if (!BinaryContent && SourceEncoding == null && Source != null && Source.Length > 0)
            {
                SourceEncoding = FileUtils.GetEncoding(Source);
            }

            base.OnExecutionStarted();
        }

        /// <summary>
        /// Overrides <see cref="CathodeRayPage.OnPrintStarted"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            _scrollBreak = ScreenIO.ScrollBreak;
            ScreenIO.ScrollBreak = true;

            Menu.Clear();

            if (!BinaryContent && SourceEncoding != null)
            {
                Menu.Add(new MenuItem("Options"));
                Menu.Add(new MenuItem(LineNumOpt, "Show Line Numbers", ShowLineNumbers, LineNumHandler));
            }

            base.OnPrintStarted();
        }

        /// <summary>
        /// Overrides: <see cref="TextPage.PrintMain"/>.
        /// </summary>
        protected override void PrintMain()
        {
            if (Source != null)
            {
                ScreenIO.PrintLn();

                try
                {
                    if (SourceEncoding != null)
                    {
                        PrintSourceText(SourceEncoding);
                    }
                    else
                    {
                        PrintSourceBinary();
                    }

                    ScreenIO.PrintLn("[EOF]", ColorId.Gray);
                }
                catch (Exception e)
                {
                    ScreenIO.PrintException(e, !(e is IOException));
                }
            }
            else
            {
                ScreenIO.PrintLn("[No Source]", ScreenOptions.Center);
            }

            base.PrintMain();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnPrintFinished"/>.
        /// </summary>
        protected override void OnPrintFinished()
        {
            base.OnPrintFinished();
            ScreenIO.ScrollBreak = _scrollBreak;
        }


        private static long GetStreamLineCount(Stream stream, long max)
        {
            long count = 0;

            if (max > 0 && stream.Length > 0)
            {
                int rc = 1;
                var buf = new byte[4096];
                stream.Seek(0, SeekOrigin.Begin);

                count = 1;

                while (rc > 0 && count < max)
                {
                    rc = stream.Read(buf, 0, buf.Length);

                    for (int n = 0; n < rc; ++n)
                    {
                        if (buf[n] == '\n') count += 1;
                    }
                }
            }

            return Math.Min(count, max);
        }

        private static string BytesToHex(byte[] buffer, int count)
        {
            var sb = new StringBuilder();

            for (int n = 0; n < count; ++n)
            {
                sb.Append(buffer[n].ToString("X2") + " ");
            }

            return sb.ToString().PadRight(buffer.Length * CharsPerByte);
        }

        private static string BytesToAscii(byte[] buffer, int count)
        {
            var sb = new StringBuilder();

            for (int n = 0; n < count; ++n)
            {
                byte b = buffer[n];

                if (b >= 0x20 && b < 0x7F)
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.Append('.');
                }
            }

            return sb.ToString().PadRight(buffer.Length);
        }

        private PageLogic LineNumHandler(MenuItem _)
        {
            ShowLineNumbers = !ShowLineNumbers;
            return PageLogic.Reprint;
        }

        private void PrintSourceText(Encoding enc)
        {
            if (Source == null)
            {
                return;
            }

            long lineNum = 0;
            long srcLen = Source.Length;
            long lineMax = GetStreamLineCount(Source, MaxLineCount);

            Source.Seek(0, SeekOrigin.Begin);

            // Leave stream open
            using (var reader = new StreamReader(Source, enc, false, 1024, true))
            {
                string? line;

                do
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        lineNum += 1;

                        if (ScreenIO.ScrollBreak)
                        {
                            if (lineMax > 0 && lineMax < MaxLineCount)
                            {
                                // Percent on line
                                ScreenIO.ScrollProgress = 100.0 * lineNum / lineMax;
                            }
                            else
                            {
                                // Percent on stream (inaccurate for short streams)
                                ScreenIO.ScrollProgress = 100.0 * Source.Position / srcLen;
                            }
                        }

                        if (ShowLineNumbers)
                        {
                            if (lineNum <= lineMax)
                            {
                                int pad = lineMax.ToString().Length;
                                ScreenIO.Print(lineNum.ToString().PadLeft(pad) + " ", ColorId.Gray);
                            }
                            else
                            {
                                // Pad only
                                ScreenIO.Print(new string(' ', lineMax.ToString().Length + 1));
                            }
                        }

                        ScreenIO.PrintLn(line);
                    }

                } while (line != null && !ScreenIO.IsPrintCancelled);
            }
        }

        private void PrintSourceBinary()
        {
            if (Source == null)
            {
                return;
            }

            Source.Seek(0, SeekOrigin.Begin);

            int blockSize = CalcLineBlock(out bool marginOn, out bool asciiColumnOn);
            var buffer = new byte[blockSize];

            uint byteCount = 0;
            long srcLen = Source.Length;
            bool headerPrinted = false;

            while (!ScreenIO.IsPrintCancelled)
            {
                int rc = Source.Read(buffer, 0, buffer.Length);

                if (rc > 0)
                {
                    if (ScreenIO.ScrollBreak)
                    {
                        ScreenIO.ScrollProgress = (int)(100.0 * Source.Position / srcLen);
                    }

                    if (!headerPrinted)
                    {
                        if (marginOn)
                        {
                            ScreenIO.Print(new string(' ', BinaryMarginWidth), ColorId.Gray);
                        }

                        int num = 0;
                        for (int n = 0; n < blockSize; ++n)
                        {
                            if (num == BlockSize) num = 0;
                            ScreenIO.Print(num++.ToString("x2") + " ", ColorId.Gray);
                        }

                        ScreenIO.PrintLn();
                        headerPrinted = true;
                    }

                    if (marginOn)
                    {
                        ScreenIO.Print(byteCount.ToString("x8") + " ", ColorId.Gray);

                        // Allow to overflow if user wants to scroll through 4GB of binary
                        byteCount += (uint)rc;
                    }

                    ScreenIO.Print(BytesToHex(buffer, rc));

                    if (asciiColumnOn)
                    {
                        ScreenIO.Print(new string(' ', AsciiSpacer) + BytesToAscii(buffer, rc));
                    }

                    ScreenIO.PrintLn();
                }
                else
                {
                    break;
                }
            }
        }

        private int CalcLineBlock(out bool marginOn, out bool asciiColumnOn)
        {
            // Returns number of bytes to read and display on single line.
            int CharsPerBlock = BlockSize * CharsPerByte;

            // <-mar-><-16*3-><-16->
            int remain = ScreenIO.ActualWidth - CharsPerBlock - BinaryMarginWidth;

            // Have width for margin?
            marginOn = remain >= 0;

            // Have remaining width for ascii characters?
            asciiColumnOn = remain >= BlockSize + AsciiSpacer;

            // Minimum plus multiples
            return BlockSize * (1 + Math.Max(remain, 0) / (CharsPerBlock + BlockSize));
        }

    }
}
