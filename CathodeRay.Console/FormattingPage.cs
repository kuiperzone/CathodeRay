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

using KuiperZone.CathodeRay;

namespace KuiperZone.CathodeRay.Console
{
    class FormattingPage : CathodeRayPage
    {
        private int _printWidth;

        public FormattingPage(CathodeRayPage parent, string title = "Formatting")
            : base(parent, title)
        {
        }

        protected override void OnExecutionStarted()
        {
            _printWidth = ScreenIO.FormatWidth;
            ScreenIO.FormatWidth = 50;

            base.OnExecutionStarted();
        }

        protected override void OnExecutionFinished()
        {
            base.OnExecutionFinished();
            ScreenIO.FormatWidth = _printWidth;
        }

        protected override void PrintMain()
        {
            const string ShortText = "The quick brown.";
            const string LongText = ShortText + " " + ShortText + " " + ShortText
                + " " + ShortText + " " + ShortText;

            ScreenIO.PrintLn();

            ScreenIO.PrintLn("NORMAL:", ColorId.Gray);
            ScreenIO.PrintLn(ShortText);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("ALIGN CENTER:", ColorId.Gray);
            ScreenIO.PrintLn(ShortText, ScreenOptions.Center);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("ALIGN RIGHT:", ColorId.Gray);
            ScreenIO.PrintLn(ShortText, ScreenOptions.Right);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("BLOCK WRAP:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.BlockWrap);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("WORD WRAP:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.WordWrap);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("BLOCK WRAP (no break):", ColorId.Gray);
            ScreenIO.PrintLn(new string('x', 3 * ScreenIO.ActualWidth / 2), ScreenOptions.BlockWrap);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("WORD WRAP (no break):", ColorId.Gray);
            ScreenIO.PrintLn(new string('x', 3 * ScreenIO.ActualWidth / 2), ScreenOptions.WordWrap);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("BLOCK WRAP (characters):", ColorId.Gray);
            for (int n = 0; n < 3 * ScreenIO.ActualWidth / 2; ++n)
            {
                ScreenIO.Print((n % 10).ToString(), ScreenOptions.BlockWrap);
            }
            ScreenIO.PrintLn();

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("WORD WRAP (characters):", ColorId.Gray);
            for (int n = 0; n < 3 * ScreenIO.ActualWidth / 2; ++n)
            {
                ScreenIO.Print((n % 10).ToString(), ScreenOptions.WordWrap);
            }
            ScreenIO.PrintLn();

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("BLOCK WRAP + CENTER:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.BlockWrap | ScreenOptions.Center);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("WORD WRAP + CENTER:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.WordWrap | ScreenOptions.Center);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("BLOCK WRAP + RIGHT:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.BlockWrap | ScreenOptions.Right);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("WORD WRAP + RIGHT:", ColorId.Gray);
            ScreenIO.PrintLn(LongText, ScreenOptions.WordWrap | ScreenOptions.Right);


            ScreenIO.PrintLn();
            ScreenIO.PrintLn("END OVERRUN:", ColorId.Gray);
            ScreenIO.PrintLn(LongText + LongText + LongText, ScreenOptions.NoWrap);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("TABS:", ColorId.Gray);
            ScreenIO.PrintLn("\t1\t2x\t3xx\t4xxx\t5xxxx\t6\nx\t7", ScreenOptions.BlockWrap);


            ScreenIO.PrintLn();
            ScreenIO.PrintLn("TRUNCATE UTILS", ColorId.Gray);

            ScreenIO.Print("Truncate(0123xxx, 4, " + Truncation.Simple + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("0123xxx", 4, Truncation.Simple));

            ScreenIO.Print("Truncate(0123xxx, 4, " + Truncation.EllipsesEnd + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("0123xxx", 4, Truncation.EllipsesEnd));

            ScreenIO.Print("Truncate(01234567, 5, " + Truncation.EllipsesCenter + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("01234567", 5, Truncation.EllipsesCenter));

            ScreenIO.Print("Truncate(01234567, 6, " + Truncation.EllipsesCenter + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("01234567", 6, Truncation.EllipsesCenter));


            ScreenIO.Print("Truncate(012345678, 7, " + Truncation.EllipsesCenter + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("012345678", 7, Truncation.EllipsesCenter));

            ScreenIO.Print("Truncate(0123456789, 7, " + Truncation.EllipsesCenter + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("0123456789", 7, Truncation.EllipsesCenter));

            ScreenIO.Print("Truncate(0123456789, 8, " + Truncation.EllipsesCenter + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("0123456789", 8, Truncation.EllipsesCenter));

            ScreenIO.Print("Truncate(0123456789, 9, " + Truncation.EllipsesCenter + ": ");
            ScreenIO.PrintLn(ScreenIO.Truncate("0123456789", 9, Truncation.EllipsesCenter));

            base.PrintMain();
        }

    }
}
