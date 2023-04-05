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

namespace KuiperZone.CathodeRay.Pages
{
    /// <summary>
    /// Displays text content with optional line numbers.
    /// </summary>
    public class TextPage : CathodeRayPage
    {
        private const string LineNumOpt = "L";
        private const int MaxLineCount = 999999;

        private bool _scrollBreak;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TextPage(CathodeRayPage parent, string? title = "Text")
            : base(parent, title)
        {
        }

        /// <summary>
        /// The text to be printed when <see cref="CathodeRayPage.Execute"/> is called. Assigning null
        /// prints nothing.
        /// </summary>
        public string? Text { get; set; } = "";

        /// <summary>
        /// Gets or sets whether line numbers are shown. Default is true.
        /// </summary>
        public bool ShowLineNumbers { get; set; } = true;

        /// <summary>
        /// Sets the <see cref="Text"/> property using a string sequence. Items will be appended
        /// with a new line character and assigned to <see cref="Text"/> as a joined string. If text
        /// is null, <see cref="Text"/> is assigned null.
        /// </summary>
        public void SetText(IEnumerable<string> text)
        {
            if (text != null)
            {
                var sb = new StringBuilder();

                foreach (var s in text)
                {
                    if (s != null)
                    {
                        sb.Append(s + "\n");
                    }
                }

                Text = sb.ToString();
            }
            else
            {
                Text = null;
            }
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnPrintStarted"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            _scrollBreak = ScreenIO.ScrollBreak;
            ScreenIO.ScrollBreak = true;

            Menu.Clear();
            Menu.Add(new MenuItem("Options"));
            Menu.Add(new MenuItem(LineNumOpt, "Show Line Numbers", ShowLineNumbers, LineNumHandler));

            base.OnPrintStarted();
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.PrintMain"/>.
        /// </summary>
        protected override void PrintMain()
        {
            if (Text != null)
            {
                ScreenIO.PrintLn();
                var lines = ScreenIO.SplitLines(Text);

                if (lines.Length > 0)
                {
                    for (int n = 0; n < lines.Length; ++n)
                    {
                        PrintNumberedLine(n + 1, lines.Length, lines[n]);
                    }
                }
                else
                {
                    ScreenIO.PrintLn("[Empty]", ScreenOptions.Center);
                }
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


        private void PrintNumberedLine(int n, int length, string text)
        {
            if (text != null)
            {
                if (n >= 0 && length > 0)
                {
                    ScreenIO.ScrollProgress = 100.0 * n / length;
                }

                if (ShowLineNumbers)
                {
                    if (n <= MaxLineCount)
                    {
                        int pad = length.ToString().Length;
                        ScreenIO.Print(n.ToString().PadLeft(pad) + " ", ColorId.Gray);
                    }
                    else
                    {
                        ScreenIO.Print(new string(' ', MaxLineCount.ToString().Length + 1));
                    }
                }

                ScreenIO.PrintLn(text);
            }
        }

        private PageLogic LineNumHandler(MenuItem _)
        {
            ShowLineNumbers = !ShowLineNumbers;
            return PageLogic.Reprint;
        }
    }
}
