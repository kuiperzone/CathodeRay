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
using KuiperZone.CathodeRay.Pages;

namespace KuiperZone.CathodeRay.Console
{
    class StaticTestPage : CathodeRayPage
    {
        public StaticTestPage(CathodeRayPage parent, string title = "Static Tests")
            : base(parent, title)
        {
            Menu.Add(new MenuItem(1, "Formatting", FormattingHandler));
            Menu.Add(new MenuItem(2, "Scrolling", ScrollingHandler));
            Menu.Add(new MenuItem(3, nameof(TextPage) + " Class", TextHandler));
        }

        private PageLogic FormattingHandler(MenuItem _)
        {
            new FormattingPage(this).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic ScrollingHandler(MenuItem _)
        {
            new ScrollingPage(this).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic TextHandler(MenuItem _)
        {
            var page = new TextPage(this)
            {
                Text = "Hello\nworld.\nThe quick brown fox jumped over the lazy dogs."
            };

            page.Execute();
            return PageLogic.Reprint;
        }

    }
}
