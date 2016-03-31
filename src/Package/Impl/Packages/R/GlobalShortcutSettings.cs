// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using EnvDTE;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal static class GlobalShortcutSettings {
        private static string[] _commands = new string[] {
            "Edit.LineOpenAbove", // Make Ctrl+Enter work in R Editor
            "Edit.LineCut",       // Make Ctrl+L work in editor 
        };
        private static object[] _existingBindings = new object[_commands.Length];

        public static void SetBindings() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            var empty = new object[0];
            for(int i = 0; i < _commands.Length; i++) {
                var command = dte.Commands.Item(_commands[i]);
                if (command != null) {
                    _existingBindings[i] = command.Bindings;
                    command.Bindings = empty;
                }
            }
        }

        public static void RestoreBindings() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            for (int i = 0; i < _commands.Length; i++) {
                if (_existingBindings[i] != null) {
                    var command = dte.Commands.Item(_commands[i]);
                    if (command != null) {
                        _existingBindings[i] = command.Bindings;
                        command.Bindings = _existingBindings[i];
                    }
                }
            }
        }
    }
}
