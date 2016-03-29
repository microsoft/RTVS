// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal static class ReplShortcutSetting {
        private static Dictionary<string, string> _shortcutRemaps = new Dictionary<string, string>() {
            { "Edit.LineOpenAbove", "ctrl+k,ctrl+a" },
            { "Edit.LineCut", "ctrl+k,ctrl+x" }
        };

        public static void Initialize() {
            SetBinding();
        }

        private static void SetBinding() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            try {
                foreach (var kvp in _shortcutRemaps) {
                    var command = dte.Commands.Item(kvp.Key);
                    if (command != null) {
                        command.Bindings = Invariant($"Text Editor::{kvp.Value}");
                    }
                }
            } catch (ArgumentException) { } catch(COMException) { }
        }
    }
}
