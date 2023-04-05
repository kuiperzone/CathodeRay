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
    class BrowserTestPage : CathodeRayPage
    {
        private string? _fileSelected;
        private string? _fileSaveAs;
        private string? _dirSelected;

        public BrowserTestPage(CathodeRayPage parent, string title = nameof(FileBrowserPage))
            : base(parent, title)
        {
        }

        /// <summary>
        /// Overrides: <see cref="CathodeRayPage.OnPrintStarted"/>.
        /// </summary>
        protected override void OnPrintStarted()
        {
            Menu.Clear();
            base.OnPrintStarted();
        }

        protected override void PrintMain()
        {
            int n = 1;
            Menu.Add(new MenuItem("File Browser"));
            Menu.Add(new MenuItem(n++, BrowserStyles.SubFileBrowser + "|" + BrowserStyles.FileViewContent, BrowserHandler1));
            Menu.Add(new MenuItem(n++, BrowserStyles.RootFileBrowser + "|" + BrowserStyles.FileViewContent, BrowserHandler2));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalFileBrowser + "|" + BrowserStyles.FullRenameDelete + " + 1 day", BrowserHandler3));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalFileBrowser + "|" + BrowserStyles.FileViewContent, BrowserHandler4));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalFileBrowser + "|" + BrowserStyles.FileViewContent + "|" + BrowserStyles.DirectToFile, BrowserHandler5));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalFileBrowser + "|" + BrowserStyles.FileOpenRun, BrowserHandler6));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalFileBrowser + "|" + BrowserStyles.FileOpenRun + "|" + BrowserStyles.DirectToFile, BrowserHandler7));

            Menu.Add(null);
            Menu.Add(new MenuItem("File Selector: " + _fileSelected));
            Menu.Add(new MenuItem(n++, BrowserStyles.SubFileSelector.ToString(), FileSelectorHandler1));
            Menu.Add(new MenuItem(n++, BrowserStyles.RootFileSelector.ToString(), FileSelectorHandler2));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalFileSelector.ToString(), FileSelectorHandler3));

            Menu.Add(null);
            Menu.Add(new MenuItem("File SaveAs: " + _fileSaveAs));
            Menu.Add(new MenuItem(n++, BrowserStyles.SubSaveAs.ToString(), SaveAsHandler1));
            Menu.Add(new MenuItem(n++, BrowserStyles.RootSaveAs.ToString(), SaveAsHandler2));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalSaveAs.ToString(), SaveAsHandler3));

            Menu.Add(null);
            Menu.Add(new MenuItem("Directory Selector: " + _dirSelected));
            Menu.Add(new MenuItem(n++, BrowserStyles.SubDirectorySelector.ToString(), DirectorySelectorHandler1));
            Menu.Add(new MenuItem(n++, BrowserStyles.RootDirectorySelector.ToString(), DirectorySelectorHandler2));
            Menu.Add(new MenuItem(n++, BrowserStyles.GlobalDirectorySelector.ToString(), DirectorySelectorHandler3));

            base.PrintMain();
        }

        private PageLogic BrowserHandler1(object _)
        {
            new FileBrowserPage(this, BrowserStyles.SubFileBrowser | BrowserStyles.FileViewContent).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler2(object _)
        {
            new FileBrowserPage(this, BrowserStyles.RootFileBrowser | BrowserStyles.FileViewContent).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler3(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.GlobalFileBrowser | BrowserStyles.FullRenameDelete)
            {
                HighlightAge = TimeSpan.FromDays(1)
            };
            page.Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler4(object _)
        {
            new FileBrowserPage(this, BrowserStyles.GlobalFileBrowser | BrowserStyles.FileViewContent).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler5(object _)
        {
            new FileBrowserPage(this, BrowserStyles.GlobalFileBrowser | BrowserStyles.FileViewContent | BrowserStyles.DirectToFile).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler6(object _)
        {
            new FileBrowserPage(this, BrowserStyles.GlobalFileBrowser | BrowserStyles.FileOpenRun).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic BrowserHandler7(object _)
        {
            new FileBrowserPage(this, BrowserStyles.GlobalFileBrowser | BrowserStyles.FileOpenRun | BrowserStyles.DirectToFile).Execute();
            return PageLogic.Reprint;
        }

        private PageLogic FileSelectorHandler1(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.SubFileSelector);
            page.Execute();
            _fileSelected = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic FileSelectorHandler2(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.RootFileSelector);
            page.Execute();
            _fileSelected = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic FileSelectorHandler3(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.GlobalFileSelector);
            page.Execute();
            _fileSelected = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic SaveAsHandler1(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.SubSaveAs);
            page.Execute();
            _fileSaveAs = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic SaveAsHandler2(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.RootSaveAs);
            page.Execute();
            _fileSaveAs = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic SaveAsHandler3(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.GlobalSaveAs);
            page.Execute();
            _fileSaveAs = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic DirectorySelectorHandler1(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.SubDirectorySelector);
            page.Execute();
            _dirSelected = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic DirectorySelectorHandler2(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.RootDirectorySelector);
            page.Execute();
            _dirSelected = page.SelectedItem;
            return PageLogic.Reprint;
        }

        private PageLogic DirectorySelectorHandler3(object _)
        {
            var page = new FileBrowserPage(this, BrowserStyles.GlobalDirectorySelector);
            page.Execute();
            _dirSelected = page.SelectedItem;
            return PageLogic.Reprint;
        }

    }
}
