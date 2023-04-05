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
using KuiperZone.CathodeRay;

namespace KuiperZone.CathodeRay.Console
{
    class PrompterTestPage : CathodeRayPage
    {
        public PrompterTestPage(CathodeRayPage parent, string title = nameof(PrompterTestPage))
            : base(parent, title)
        {
            int n = 1;
            Menu.Add(new MenuItem("Strings"));
            Menu.Add(new MenuItem(n++, nameof(PromptStyle.Text), TextHandler));
            Menu.Add(new MenuItem(n++, nameof(PromptStyle.Text) + " 6, 10", Text610Handler));
            Menu.Add(new MenuItem(n++, nameof(PromptStyle.HidePassword), PasswordHandler));
            Menu.Add(new MenuItem(n++, nameof(PromptStyle.FileName), FileNameHandler));
            Menu.Add(new MenuItem(n++, nameof(PromptStyle.FilePath), FilePathHandler));

            Menu.Add(null);
            Menu.Add(new MenuItem("Numbers"));
            Menu.Add(new MenuItem(n++, "Bool", BooleanHandler));
            Menu.Add(new MenuItem(n++, "Int32", IntHandler));
            Menu.Add(new MenuItem(n++, "Double", DoubleHandler));

            Menu.Add(null);
            Menu.Add(new MenuItem("Other"));
            Menu.Add(new MenuItem(n++, nameof(PromptStyle.Confirm), ConfirmHandler));
            Menu.Add(new MenuItem(n++, "ISO8601 Date/time", DateTimeHandler));
        }

        private PageLogic TextHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(PromptStyle.Text), "Hello world");
        }

        private PageLogic Text610Handler(MenuItem _)
        {
            var prompt = new Prompter(PromptStyle.Text)
            {
                MinLength = 6,
                MaxLength = 10
            };
            return ExecutePrompt(prompt);
        }

        private PageLogic PasswordHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(PromptStyle.HidePassword));
        }

        private PageLogic FileNameHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(PromptStyle.FileName));
        }

        private PageLogic FilePathHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(PromptStyle.FilePath));
        }

        private PageLogic BooleanHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(typeof(bool)));
        }

        private PageLogic IntHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(typeof(int)));
        }

        private PageLogic DoubleHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(typeof(double)));
        }

        private PageLogic ConfirmHandler(MenuItem _)
        {
            return ExecutePrompt(new Prompter(PromptStyle.Confirm));
        }

        private PageLogic DateTimeHandler(MenuItem _)
        {
            var prompt = new Prompter(typeof(DateTime))
            {
                LegalChars = "0123456789Z",
                LegalFilter = "????-??-??T??:??*",
                MaxLength = 17,
                IgnoreLegalCase = true,
                Prefix = "Date? [yyyy-MM-ddTHH:mm]: "
            };
            return ExecutePrompt(prompt);
        }

        private PageLogic ExecutePrompt(Prompter prompt, string? seed = null)
        {
            ScreenIO.PrintLn();
            ScreenIO.PrintLn("Prompt  : " + prompt.Style);

            prompt.Execute(seed);

            ScreenIO.PrintLn();
            ScreenIO.PrintLn("Status : " + prompt.Status);
            ScreenIO.PrintLn("Result : " + (prompt.InputString ?? "{null}"));

            if (prompt.TryResult(out object? value))
            {
                ScreenIO.PrintLn("Value  : " + (value?.ToString() ?? "{null}"));
            }
            else
            {
                ScreenIO.PrintLn("Value  : FAILED", ColorId.Critical);
            }

            ScreenIO.PrintLn();
            new Prompter(PromptStyle.AnyKey).Execute();
            return PageLogic.Reprint;
        }
    }
}
