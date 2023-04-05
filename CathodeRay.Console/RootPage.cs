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
using KuiperZone.CathodeRay.Pages;
using KuiperZone.CathodeRay.Themes;

namespace KuiperZone.CathodeRay.Console
{
    /// <summary>
    /// Only a form of "static testing".
    /// </summary>
    class RootPage : CathodeRayPage
    {
        public RootPage()
            : base("HOME MENU")
        {
            ScreenIO.Theme = new TronTheme();
            PrintStarted += PrintHeader;

            int n = 1;
            Menu.Add(new MenuItem(n++, "Settings", SettingsHandler));
            Menu.Add(new MenuItem(n++, "Static Tests", StaticTestsHandler));
            Menu.Add(new MenuItem(n++, "Prompter", PrompterHandler));
            Menu.Add(new MenuItem(n++, "File Browser", BrowserHandler));
        }

        private PageLogic SettingsHandler(MenuItem _)
        {
            new ScreenSettingsPage(this).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic StaticTestsHandler(MenuItem _)
        {
            new StaticTestPage(this).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic PrompterHandler(MenuItem _)
        {
            new PrompterTestPage(this).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler(MenuItem _)
        {
            new BrowserTestPage(this).Execute();
            return PageLogic.Reprint;
        }

        private void PrintHeader(object? sender, EventArgs e)
        {
            ScreenIO.PrintLn(new string('-', ScreenIO.ActualWidth));

            ScreenIO.PrintLn(ScreenIO.DoubleSpace("CATHODE RAY"));

            ScreenIO.PrintLn(new string('-', ScreenIO.ActualWidth));
            ScreenIO.PrintLn();
        }
    }
}
