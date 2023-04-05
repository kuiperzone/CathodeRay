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

using System;
using Xunit;

namespace KuiperZone.CathodeRay.Test
{
    /// <summary>
    /// Test all we can without printing to Console.
    /// </summary>
    public class ScreenIOTest
    {
        [Fact]
        public void Truncate_Simple_HandlesNullAndEmpty()
        {
            Assert.Null(ScreenIO.Truncate(null, 5, Truncation.Simple));
            Assert.Empty(ScreenIO.Truncate("", 5, Truncation.Simple)!);
        }

        [Fact]
        public void Truncate_Simple_DoesNotTruncateShort()
        {
            // No truncation
            Assert.Equal("123", ScreenIO.Truncate("123", 3, Truncation.Simple));
            Assert.Equal("123", ScreenIO.Truncate("123", 4, Truncation.Simple));
        }

        [Fact]
        public void Truncate_Simple_IsCorrect()
        {
            Assert.Equal("12", ScreenIO.Truncate("1234567890", 2, Truncation.Simple));
            Assert.Equal("123", ScreenIO.Truncate("1234567890", 3, Truncation.Simple));
        }

        [Fact]
        public void Truncate_EllipsesEnd_HandlesNullAndEmpty()
        {
            Assert.Null(ScreenIO.Truncate(null, 5, Truncation.EllipsesEnd));
            Assert.Empty(ScreenIO.Truncate("", 5, Truncation.EllipsesEnd)!);
        }

        [Fact]
        public void Truncate_EllipsesEnd_DoesNotTruncateShort()
        {
            Assert.Equal("123", ScreenIO.Truncate("123", 4, Truncation.EllipsesEnd));
            Assert.Equal("123", ScreenIO.Truncate("123", 5, Truncation.EllipsesEnd));
        }

        [Fact]
        public void Truncate_EllipsesEnd_IsCorrect()
        {
            Assert.Equal("1...", ScreenIO.Truncate("1234567890", 4, Truncation.EllipsesEnd));
            Assert.Equal("123...", ScreenIO.Truncate("1234567890", 6, Truncation.EllipsesEnd));
        }

        [Fact]
        public void Truncate_EllipsesEnd_FallsBackToSimple()
        {
            Assert.Equal("12", ScreenIO.Truncate("1234567890", 2, Truncation.EllipsesEnd));
            Assert.Equal("123", ScreenIO.Truncate("1234567890", 3, Truncation.EllipsesEnd));
        }

        [Fact]
        public void Truncate_EllipsesCenter_HandlesNullAndEmpty()
        {
            Assert.Null(ScreenIO.Truncate(null, 5, Truncation.EllipsesCenter));
            Assert.Empty(ScreenIO.Truncate("", 5, Truncation.EllipsesCenter)!);
        }

        [Fact]
        public void Truncate_EllipsesCenter_DoesNotTruncateShort()
        {
            Assert.Equal("0123", ScreenIO.Truncate("0123", 4, Truncation.EllipsesCenter));
            Assert.Equal("0123", ScreenIO.Truncate("0123", 5, Truncation.EllipsesCenter));
        }

        [Fact]
        public void Truncate_EllipsesCenter_IsCorrect()
        {
            // 01234567   -> 0...7 [5]
            // 01234567   -> 01...7 [6]
            Assert.Equal("0...7", ScreenIO.Truncate("01234567", 5, Truncation.EllipsesCenter));
            Assert.Equal("01...7", ScreenIO.Truncate("01234567", 6, Truncation.EllipsesCenter));

            // 012345678  -> 01...78 [7]
            // 0123456789 -> 01...89 [7]
            Assert.Equal("01...78", ScreenIO.Truncate("012345678", 7, Truncation.EllipsesCenter));
            Assert.Equal("01...89", ScreenIO.Truncate("0123456789", 7, Truncation.EllipsesCenter));

            // 0123456789 -> 012...89 [8]
            // 0123456789 -> 012...789 [9]
            Assert.Equal("012...89", ScreenIO.Truncate("0123456789", 8, Truncation.EllipsesCenter));
            Assert.Equal("012...789", ScreenIO.Truncate("0123456789", 9, Truncation.EllipsesCenter));
        }

        [Fact]
        public void Truncate_EllipsesCenter_FallsBackToEnd()
        {
            Assert.Equal("0...", ScreenIO.Truncate("01234567", 4, Truncation.EllipsesCenter));

            // To simple
            Assert.Equal("012", ScreenIO.Truncate("01234567", 3, Truncation.EllipsesCenter));
        }

        [Fact]
        public void DoubleSpace_HandlesNullAndEmpty()
        {
            Assert.Null(ScreenIO.DoubleSpace(null));
            Assert.Equal("", ScreenIO.DoubleSpace(""));
        }

        [Fact]
        public void DoubleSpace_IsCorrect()
        {
            Assert.Equal("A", ScreenIO.DoubleSpace("A"));
            Assert.Equal("A D", ScreenIO.DoubleSpace("AD"));
            Assert.Equal("H E L L O", ScreenIO.DoubleSpace("HELLO"));
            Assert.Equal("1 9 { H E L L O } 7 7", ScreenIO.DoubleSpace("19{HELLO}77"));
        }

        [Fact]
        public void DoubleSpace_HandlesBreak()
        {
            Assert.Equal("A D A\nP A", ScreenIO.DoubleSpace("ADA\nPA"));
        }

        [Fact]
        public void SliptLines_HandlesNullAndEmpty()
        {
            Assert.NotNull(ScreenIO.SplitLines(null));
            Assert.Empty(ScreenIO.SplitLines(null));

            Assert.NotNull(ScreenIO.SplitLines(""));
            Assert.Empty(ScreenIO.SplitLines(""));
        }

        [Fact]
        public void SliptLines_NoSplit()
        {
            var lines = ScreenIO.SplitLines("123");
            Assert.Single(lines);
            Assert.Equal("123", lines[0]);
        }

        [Fact]
        public void SliptLines_HandlesFeed()
        {
            var lines = ScreenIO.SplitLines("456" + '\x0C' + "789\n");

            foreach (var line in lines)
            {
                if (line == null)
                {
                    Console.WriteLine("{null}");
                }
                else
                if (line == "")
                {
                    Console.WriteLine("{empty}");
                }
                else
                {
                    Console.WriteLine(line);
                }
            }

            Assert.Equal(4, lines.Length);

            Assert.Equal("456", lines[0]);
            Assert.Equal("", lines[1]);

            Assert.Equal("789", lines[2]);
            Assert.Equal("", lines[3]);
        }

        [Fact]
        public void SliptLines_HandlesNewLine()
        {
            // Expect trim CR
            var lines = ScreenIO.SplitLines("123 \n 456\r\n789\n");
            Assert.Equal(4, lines.Length);

            Assert.Equal("123 ", lines[0]);
            Assert.Equal(" 456", lines[1]);
            Assert.Equal("789", lines[2]);
            Assert.Equal("", lines[3]);
        }


   }
}