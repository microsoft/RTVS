// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal static class ReplShortcutSetting {
        private static Dictionary<string, string> _editorShortcutRemaps = new Dictionary<string, string>() {
            { "Edit.LineOpenAbove", "Text Editor::ctrl+k,ctrl+a" }, // Make Ctrl+Enter work in R Editor
            { "Edit.LineCut", "Text Editor::ctrl+k,ctrl+x" }        // Make Ctrl+L work in editor 
        };
        private static Dictionary<string, string> _globalShortcutRemaps = new Dictionary<string, string>() {
            { "Edit.LineCut", "global::ctrl+k,ctrl+x" }             // Make Ctrl+L work globally 
        };

        public static void Initialize() {
            SetBinding();
        }

        private static void SetBinding() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            try {
                dte.ApplyShortcuts(_editorShortcutRemaps);
                dte.ApplyShortcuts(_globalShortcutRemaps);
            } catch (ArgumentException) { } catch(COMException) { }
        }

        private static void ApplyShortcuts(this DTE dte, IReadOnlyDictionary<string, string> shortcuts) {
            foreach (var kvp in shortcuts) {
                var command = dte.Commands.Item(kvp.Key);
                if (command != null) {
                    command.Bindings = kvp.Value;
                }
            }
        }
    }
}
