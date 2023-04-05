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

namespace KuiperZone.CathodeRay.Console
{
    class ScrollingPage : CathodeRayPage
    {
        private bool _scrolMore;
        private int _printWidth;

        public ScrollingPage(CathodeRayPage parent, string title = "Scrolling")
            : base(parent, title)
        {
        }

        protected override void OnExecutionStarted()
        {
            _scrolMore = ScreenIO.ScrollBreak;
            _printWidth = ScreenIO.FormatWidth;

            ScreenIO.ScrollBreak = true;
            ScreenIO.FormatWidth = 50;

            base.OnExecutionStarted();
        }

        protected override void OnExecutionFinished()
        {
            base.OnExecutionFinished();

            ScreenIO.ScrollBreak = _scrolMore;
            ScreenIO.FormatWidth = _printWidth;
        }

        protected override void PrintMain()
        {
            ScreenIO.PrintLn();

            int y = 0;

            for (int n = 0; n < System.Console.WindowHeight * 2; ++n)
            {
                ScreenIO.PrintLn(y++.ToString() + ", LineCount: " + ScreenIO.LineCount);
            }

            int countX = System.Console.WindowWidth + 10;
            ScreenIO.Print(new string('a', countX));
            ScreenIO.Print(new string('b', countX));

            ScreenIO.PrintLn(new string('x', countX));
            ScreenIO.PrintLn("LineCount: " + ScreenIO.LineCount);

            for (int n = 0; n < System.Console.WindowHeight * 2; ++n)
            {
                ScreenIO.PrintLn(y++.ToString() + ", LineCount: " + ScreenIO.LineCount);
            }

            ScreenIO.PrintLn(new string('x', countX));
            ScreenIO.PrintLn("LineCount: " + ScreenIO.LineCount);

            const string ShortText = "The quick brown.";
            const string LongText = ShortText + " " + ShortText + " " + ShortText
                + " " + ShortText + " " + ShortText;

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("BLOCK WRAP:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.BlockWrap);
            ScreenIO.PrintLn("LineCount: " + ScreenIO.LineCount);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("WORD WRAP:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.WordWrap);
            ScreenIO.PrintLn("LineCount: " + ScreenIO.LineCount);

            base.PrintMain();
        }

    }
}
