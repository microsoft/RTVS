// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using EnvDTE;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal static class ReplShortcutSetting {
        private const string CommandName = "EditorContextMenus.CodeWindow.ExecuteLineInInteractive";

        private static bool _currentSetting;

        public static void Initialize() {
            _currentSetting = REditorSettings.SendToReplOnCtrlEnter;
            REditorSettings.Changed += REditorSettings_Changed;

            SetBinding();
        }

        public static void Close() {
            REditorSettings.Changed -= REditorSettings_Changed;
        }

        private static void SetBinding() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            string binding;

            if (dte == null) {
                return;
            }

            if (REditorSettings.SendToReplOnCtrlEnter) {
                binding = "Text Editor::ctrl+enter";
            } else {
                binding = "Text Editor::ctrl+e,ctrl+e";
            }

            if (REditorSettings.SendToReplOnCtrlEnter) {
                // Find and save existing binding
                Command c = dte.Commands.Item("EditorContextMenus.CodeWindow.ExecuteLineInInteractive");
                if (c != null) {
                    object[] commandBindings = c.Bindings as object[];
                    if (commandBindings != null && commandBindings.Length > 0) {
                        string commandName = c.Name;
                        if (!commandName.ToLowerInvariant().Contains("ExecuteLineInInteractive")) {
                            foreach (object o in commandBindings) {
                                string commandBinding = o as string;
                                if (string.IsNullOrEmpty(commandBinding)) {
                                    if (commandBinding.Contains("text editor") && commandBinding.Contains("ctrl+enter")) {
                                        REditorSettings.WritableStorage.SetString("CtrlEnterBinding", commandBinding);
                                        REditorSettings.WritableStorage.SetString("CtrlEnterCommandName", c.Name);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } else {
                // Restore original binding, if any.
                string storedBinding = REditorSettings.WritableStorage.GetString("CtrlEnterBinding", string.Empty);
                string storedCommandName = REditorSettings.WritableStorage.GetString("CtrlEnterCommandName", string.Empty);
                if (!string.IsNullOrEmpty(storedCommandName) && !string.IsNullOrEmpty(storedBinding)) {
                    try {
                        Command c = dte.Commands.Item(storedCommandName);
                        if (c != null) {
                            c.Bindings = storedBinding;
                        }
                    } catch (ArgumentException) { }
                }
            }

            try {
                Command sendToReplCommand = dte.Commands.Item(CommandName);
                Debug.Assert(sendToReplCommand != null);
                if (sendToReplCommand != null) {
                    sendToReplCommand.Bindings = binding;
                }
            } catch (ArgumentException) { }
        }


        private static void REditorSettings_Changed(object sender, EventArgs e) {
            if (_currentSetting != REditorSettings.SendToReplOnCtrlEnter) {
                SetBinding();
                _currentSetting = REditorSettings.SendToReplOnCtrlEnter;
            }
        }
    }
}
