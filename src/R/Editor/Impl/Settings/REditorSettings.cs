using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.ContentType;

namespace Microsoft.R.Editor.Settings {
    public static class REditorSettings {
        public const string AutoFormatKey = "AutoFormat";
        public const string FormatOnPasteKey = "FormatOnPaste";
        public const string CommitOnSpaceKey = "CommitOnSpace";
        public const string CommitOnEnterKey = "CommitOnEnter";
        public const string CompletionOnFirstCharKey = "CompletionOnFirstChar";
        public const string SendToReplOnCtrlEnterKey = "SendToReplOnCtrlEnter";
        public const string SyntaxCheckInReplKey = "SyntaxCheckInRepl";
        public const string PartialArgumentNameMatchKey = "PartialArgumentNameMatch";

        private static bool _initialized = false;
        private static RFormatOptions _formatOptions = new RFormatOptions();

        private static ISettingsStorage Storage {
            get {
                var storage = (ISettingsStorage)EditorShell.GetSettings(RContentTypeDefinition.LanguageName);

                if (!_initialized) {
                    storage.SettingsChanged += OnSettingsChanged;
                    _initialized = true;
                }

                return storage;
            }
        }

        public static IWritableSettingsStorage WritableStorage {
            get { return Storage as IWritableSettingsStorage; }
        }

        private static bool IsWritable {
            get { return Storage is IWritableSettingsStorage; }
        }

        public static event EventHandler<EventArgs> Changed;

        public static void ResetSettings() {
            if (IsWritable)
                WritableStorage.ResetSettings();

            _formatOptions = new RFormatOptions();
        }

        private static void OnSettingsChanged(object sender, EventArgs e) {
            if (Changed != null)
                Changed(null, EventArgs.Empty);
        }

        public static bool CompletionEnabled {
            get {
                return CommonSettings.GetCompletionEnabled(Storage);
            }
        }

        public static bool SignatureHelpEnabled {
            get {
                return CommonSettings.GetSignatureHelpEnabled(Storage);
            }
        }

        public static bool SyntaxCheck {
            get {
                return CommonSettings.GetValidationEnabled(Storage);
            }

            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(CommonSettings.ValidationEnabledKey, value);
            }
        }

        public static bool InsertMatchingBraces {
            get {
                return Storage.GetBoolean(CommonSettings.InsertMatchingBracesKey, true);
            }

            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(CommonSettings.InsertMatchingBracesKey, value);
            }
        }

        public static bool FormatOnPaste {
            get {
                return Storage.GetBoolean(FormatOnPasteKey, true);
            }

            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(FormatOnPasteKey, value);
            }
        }

        public static bool AutoFormat {
            get {
                return Storage.GetBoolean(AutoFormatKey, true);
            }

            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(AutoFormatKey, value);
            }
        }

        public static bool CommitOnSpace {
            get {
                return Storage.GetBoolean(CommitOnSpaceKey, false);
            }

            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(CommitOnSpaceKey, value);
            }
        }

        public static bool CommitOnEnter {
            get { return Storage.GetBoolean(CommitOnEnterKey, false); }
            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(CommitOnEnterKey, value);
            }
        }


        public static bool ShowCompletionOnFirstChar {
            get { return Storage.GetBoolean(CompletionOnFirstCharKey, true); }
            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(CompletionOnFirstCharKey, value);
            }
        }

        public static IndentType IndentType {
            get { return (IndentType)Storage.GetInteger(CommonSettings.FormatterIndentTypeKey, (int)IndentType.Spaces); }
            set {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.FormatterIndentTypeKey, (int)value);
            }
        }

        public static int IndentSize {
            get { return Storage.GetInteger(CommonSettings.FormatterIndentSizeKey, 4); }
            set {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.FormatterIndentSizeKey, value);
            }
        }

        public static IndentStyle IndentStyle {
            get { return (IndentStyle)Storage.GetInteger(CommonSettings.IndentStyleKey, (int)IndentStyle.Smart); }
            set {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.IndentStyleKey, (int)value);
            }
        }

        public static int TabSize {
            get { return Storage.GetInteger(CommonSettings.FormatterTabSizeKey, 4); }
            set {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.FormatterTabSizeKey, value);
            }
        }

        public static bool SendToReplOnCtrlEnter {
            get {
                return Storage.GetBoolean(REditorSettings.SendToReplOnCtrlEnterKey, true);
            }

            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(REditorSettings.SendToReplOnCtrlEnterKey, value);
            }
        }

        public static bool SyntaxCheckInRepl {
            get { return Storage.GetBoolean(REditorSettings.SyntaxCheckInReplKey, false); }
            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(REditorSettings.SyntaxCheckInReplKey, false);
            }
        }

        public static bool PartialArgumentNameMatch {
            get { return Storage.GetBoolean(REditorSettings.PartialArgumentNameMatchKey, false); }
            set {
                if (IsWritable)
                    WritableStorage.SetBoolean(REditorSettings.PartialArgumentNameMatchKey, value);
            }
        }

        public static RFormatOptions FormatOptions {
            get {
                _formatOptions.IndentSize = REditorSettings.IndentSize;
                _formatOptions.IndentType = REditorSettings.IndentType;
                _formatOptions.TabSize = REditorSettings.TabSize;

                return _formatOptions;
            }
        }
    }
}
