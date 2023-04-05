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
    /// Class which prints to <see cref="ScreenIO"/> instance while holding the original cursor
    /// position on construction. On each update to <see cref="ReprinterIO.Text"/>, the screen text
    /// is replaced. This class can be used, for example, to print a progress percentage which will
    /// be overwritten (rather than amended) on update.
    /// </summary>
    public class ReprinterIO
    {
        private string? _text;

        /// <summary>
        /// Constructor with color only.
        /// </summary>
        public ReprinterIO(ColorId col = ColorId.Text)
        {
            Color = col;

            ScreenIO.Reset();
            PosXY = ScreenIO.PosXY;
        }

        /// <summary>
        /// Constructor with text string and color.
        /// </summary>
        public ReprinterIO(string text, ColorId col = ColorId.Text)
            : this(col)
        {
            Text = text;
        }

        /// <summary>
        /// Gets or sets the <see cref="ColorId"/> value.
        /// </summary>
        public ColorId Color { get; set; }

        /// <summary>
        /// Gets the <see cref="ScreenIO"/> supplied on construction.
        /// </summary>
        public Tuple<int, int> PosXY { get; }

        /// <summary>
        /// Gets or sets the text at <see cref="PosXY"/>, replacing any existing value on screen.
        /// The new text is printed with <see cref="Color"/>.
        /// </summary>
        public string? Text
        {
            get { return _text; }

            set
            {
                if (_text != null)
                {
                    Clear(_text.Length);
                }

                ScreenIO.Print(value, Color);
                _text = value;
            }
        }

        /// <summary>
        /// Clears the text and sets the <see cref="ScreenIO.PosXY"/> to <see cref="PosXY"/>.
        /// </summary>
        public void Clear()
        {
            if (_text != null)
            {
                Clear(_text.Length);
            }
        }

        private void Clear(int len)
        {
            ScreenIO.PosXY = PosXY;

            if (len > 0)
            {
                ScreenIO.Print(new string(' ', len));
                ScreenIO.PosXY = PosXY;
            }
        }
    }
}
