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
    /// A class which hold menu item data. See <see cref="CathodeRayPage.Menu"/>.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Default shortcut padding .
        /// </summary>
        public const int DefaultPadding = 3;

        /// <summary>
        /// Constructor. See <see cref="Shortcut"/>, <see cref="Text"/> and <see cref="Handler"/>.
        /// The "tag" is any object for use in <see cref="Handler"/>. The "pad" value is used to
        /// right pad the <see cref="Shortcut"/> string. The <see cref="Color"/> will be set to <see
        /// cref="ColorId.Text"/> if "handler" is not null, otherwise it will be <see
        /// cref="ColorId.Gray"/> to indicate disabled.
        /// </summary>
        /// <exception cref="ArgumentNullException">shortcut</exception>
        /// <exception cref="ArgumentException">Shortcut is empty</exception>
        public MenuItem(string shortcut, string text, LogicHandler? handler, object? tag = null, int pad = DefaultPadding)
        {
            if (shortcut == null)
            {
                throw new ArgumentNullException(nameof(shortcut));
            }

            Shortcut = shortcut.Trim();

            if (Shortcut.Length == 0)
            {
                throw new ArgumentException("Shortcut is empty");
            }

            Text = text.Trim();
            Padding = pad;
            Handler = handler;
            Color = Handler != null ? ColorId.Text : ColorId.Gray;
            Tag = tag;
        }

        /// <summary>
        /// Constructor where "shortcut" is supplied as an integer.
        /// </summary>
        /// <exception cref="ArgumentNullException">shortcut</exception>
        /// <exception cref="ArgumentException">Shortcut is empty</exception>
        public MenuItem(int shortcut, string text, LogicHandler? handler, object? tag = null, int pad = DefaultPadding)
            : this(shortcut.ToString(), text, handler, tag, pad)
        {
        }

        /// <summary>
        /// Constructor with additional "suffixed" value. See <see cref="IsSuffixed"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">shortcut</exception>
        /// <exception cref="ArgumentException">Shortcut is empty</exception>
        public MenuItem(string shortcut, string text, bool suffixed, LogicHandler? handler, object? tag = null, int pad = DefaultPadding)
            : this(shortcut, text, handler, tag, pad)
        {
            IsSuffixed = suffixed;
        }

        /// <summary>
        /// Constructor where "shortcut" is supplied as an integer.
        /// </summary>
        /// <exception cref="ArgumentNullException">shortcut</exception>
        /// <exception cref="ArgumentException">Shortcut is empty</exception>
        public MenuItem(int shortcut, string text, bool suffixed, LogicHandler? handler,
            object? tag = null, int pad = DefaultPadding)
            : this(shortcut.ToString(), text, handler, tag, pad)
        {
            IsSuffixed = suffixed;
        }

        /// <summary>
        /// Constructor. The item will serve only as a "header", i.e. it will be a <see
        /// cref="MenuItem"/> instance without <see cref="Shortcut"/> or <see cref="Handler"/> which
        /// displays only text. The default color is <see cref="ColorId.Gray"/> (disabled).
        /// </summary>
        public MenuItem(string text, ColorId color = ColorId.Gray)
        {
            Shortcut = "";
            Text = text;
            Color = color;
        }

        /// <summary>
        /// The <see cref="Handler"/> delegate type. The handler will be passed the value of
        /// <see cref="Tag"/>.
        /// </summary>
        public delegate PageLogic LogicHandler(MenuItem sender);

        /// <summary>
        /// Gets the menu shortcut. This defines input the user must enter to select this option.
        /// The value should be unique. The shortcut value is always appended with '.' and then
        /// right padded according to <see cref="Padding"/>. If <see cref="Shortcut"/> is null,
        /// only the <see cref="Text"/> is printed and, with the user unable to select this item,
        /// it will serve only as title or header text.
        /// </summary>
        public string Shortcut { get; }

        /// <summary>
        /// Gets or sets the menu option text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets the option handler which will be called if the user types <see cref="Shortcut"/>
        /// at the prompt. Note that this value will always be null if <see cref="Shortcut"/> is
        /// null or empty.
        /// </summary>
        public LogicHandler? Handler { get; }

        /// <summary>
        /// Gets or sets the option <see cref="ColorId"/> value.
        /// </summary>
        public ColorId Color { get; set; } = ColorId.Text;

        /// <summary>
        /// Gets or sets a custom "tag" value. This can be any object for use in the <see
        /// cref="LogicHandler"/> handler implementation. The default is null.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Gets the value used to right-pad <see cref="Shortcut"/> when the option is printed. The
        /// default is 3.
        /// </summary>
        public int Padding { get; }

        /// <summary>
        /// Gets or sets whether the menu option is suffixed with <see cref="CathodeRayPage.FlagSuffix"/>.
        /// This is typically used to indicate that a setting flag is "on" or "enabled".
        /// </summary>
        public bool IsSuffixed { get; set; }

        /// <summary>
        /// Prints the menu on using <see cref="ScreenIO"/>. If the item is not hidden, it is
        /// printed with a new line.
        /// </summary>
        internal void PrintLn()
        {
            var str = Shortcut;

            if (!string.IsNullOrEmpty(str))
            {
                str = (str + ". ").PadRight(Math.Max(Padding, 0));

                if (CathodeRayPage.GlobalOptions.HasFlag(PageOptions.IndentMenu))
                {
                    str = new string(' ', ScreenIO.TabSize) + str;
                }
            }

            str += Text;

            if (!string.IsNullOrEmpty(str))
            {
                ScreenIO.PrintLn(str + (IsSuffixed ? CathodeRayPage.FlagSuffix : null), Color);
            }
            else
            {
                // Vertical spacer
                ScreenIO.PrintLn();
            }
        }

    }
}
