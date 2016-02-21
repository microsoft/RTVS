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
