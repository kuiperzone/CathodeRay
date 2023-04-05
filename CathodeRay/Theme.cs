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

using KuiperZone.CathodeRay.Themes;

namespace KuiperZone.CathodeRay
{
    /// <summary>
    /// Maps <see cref="ColorId"/> values to ConsoleColors. On construction, <see cref="Theme"/>
    /// initialises colors set from the system console foreground and background values. Subclass of this
    /// class provide for customization.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// System foreground color.
        /// </summary>
        public static readonly ConsoleColor SystemForeground;

        /// <summary>
        /// System background color.
        /// </summary>
        public static readonly ConsoleColor SystemBackground;

        private readonly ConsoleColor[] _colors = InitialiseColors();

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Theme()
        {
            try
            {
                // Get the default Console colors
                Console.ResetColor();
                SystemForeground = Console.ForegroundColor;
                SystemBackground = Console.BackgroundColor;
            }
            catch
            {
                SystemForeground = ConsoleColor.White;
                SystemBackground = ConsoleColor.Black;
            }

            Themes = new List<Theme>
            {
                new Theme(),
                new RetroTheme(false),
                new RetroTheme(true),
                new TronTheme(false),
                new TronTheme(true),
                new XtalTheme()
            };
        }

        /// <summary>
        /// Returns a list of all theme instances. It will be initially be populated with a
        /// selection of supplied themes, but can be added to.
        /// </summary>
        public static IList<Theme> Themes { get; }

        /// <summary>
        /// Color scheme name.
        /// </summary>
        public string Name { get; protected set; } = "System Default";

        /// <summary>
        /// The number of colors in the color scheme.
        /// </summary>
        public int Count
        {
            get { return _colors.Length; }
        }

        /// <summary>
        /// Indexer to the color map.
        /// </summary>
        public ConsoleColor this[ColorId col]
        {
            get { return _colors[(int)col]; }
            protected set { _colors[(int)col] = value; }
        }

        /// <summary>
        /// Returns the <see cref="Theme"/> instance matching "name". If no match is found,
        /// it returns the default.
        /// </summary>
        public static Theme Get(string? name)
        {
            foreach (var t in Themes)
            {
                if (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }

            return Themes[0];
        }

        private static ConsoleColor[] InitialiseColors()
        {
            // Defines default color scheme
            var count = Enum.GetNames(typeof(ColorId)).Length;
            var col = new ConsoleColor[count];

            col[(int)ColorId.Background] = SystemBackground;
            col[(int)ColorId.Text] = SystemForeground;

            col[(int)ColorId.Title] = SystemForeground;
            col[(int)ColorId.Input] = SystemForeground;

            switch (SystemForeground)
            {
            case ConsoleColor.White:
                col[(int)ColorId.High] = ConsoleColor.Cyan;
                break;
            case ConsoleColor.Black:
            case ConsoleColor.DarkGray:
                col[(int)ColorId.High] = ConsoleColor.DarkBlue;
                break;
            default:
                col[(int)ColorId.High] = ConsoleColor.White;
                break;
            }

            col[(int)ColorId.Gray] = ConsoleColor.DarkGray;

            col[(int)ColorId.Success] = ConsoleColor.Green;
            col[(int)ColorId.Warning] = ConsoleColor.Yellow;
            col[(int)ColorId.Critical] = ConsoleColor.Red;

            return col;
        }
    }
}
