// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Formatting;

namespace Microsoft.R.Editor.Settings {
    public static class REditorSettings {
        public const string FormatScopeKey = "FormatScope";
        public const string FormatOnPasteKey = "FormatOnPaste";
        public const string CommitOnSpaceKey = "CommitOnSpace";
        public const string CommitOnEnterKey = "CommitOnEnter";
        public const string CompletionOnFirstCharKey = "CompletionOnFirstChar";
        public const string CompletionOnTabKey = "CompletionOnTab";
        public const string SendToReplOnCtrlEnterKey = "SendToReplOnCtrlEnter";
        public const string SyntaxCheckInReplKey = "SyntaxCheckInRepl";
        public const string PartialArgumentNameMatchKey = "PartialArgumentNameMatch";

        private static bool _initialized = false;
        private static RFormatOptions _formatOptions = new RFormatOptions();

        private static IEditorSettingsStorage Storage {
            get {
                var storage = (IEditorSettingsStorage)EditorShell.GetSettings(RContentTypeDefinition.LanguageName);

                if (!_initialized) {
                    storage.SettingsChanged += OnSettingsChanged;
                    _initialized = true;
                }

                return storage;
            }
        }

        public static IWritableEditorSettingsStorage WritableStorage {
            get { return Storage as IWritableEditorSettingsStorage; }
        }

        public static event EventHandler<EventArgs> Changed;

        public static void ResetSettings() {
            WritableStorage?.ResetSettings();
            _formatOptions = new RFormatOptions();
        }

        private static void OnSettingsChanged(object sender, EventArgs e) {
            Changed?.Invoke(null, EventArgs.Empty);
        }

        public static bool CompletionEnabled => CommonSettings.GetCompletionEnabled(Storage);
        public static bool SignatureHelpEnabled => CommonSettings.GetSignatureHelpEnabled(Storage);

        public static bool SyntaxCheck {
            get { return CommonSettings.GetValidationEnabled(Storage); }
            set { WritableStorage?.SetBoolean(CommonSettings.ValidationEnabledKey, value); }
        }

        public static bool InsertMatchingBraces {
            get { return Storage.GetBoolean(CommonSettings.InsertMatchingBracesKey, true); }
            set { WritableStorage?.SetBoolean(CommonSettings.InsertMatchingBracesKey, value); }
        }

        public static bool FormatOnPaste {
            get { return Storage.GetBoolean(FormatOnPasteKey, true); }
            set { WritableStorage?.SetBoolean(FormatOnPasteKey, value); }
        }

        public static bool AutoFormat {
            get { return Storage.GetBoolean(CommonSettings.AutoFormatKey, true); }
            set { WritableStorage?.SetBoolean(CommonSettings.AutoFormatKey, value); }
        }

        public static bool FormatScope {
            get { return Storage.GetBoolean(FormatScopeKey, true); }
            set { WritableStorage?.SetBoolean(FormatScopeKey, value); }
        }

        public static bool CommitOnSpace {
            get { return Storage.GetBoolean(CommitOnSpaceKey, false); }
            set { WritableStorage?.SetBoolean(CommitOnSpaceKey, value); }
        }

        public static bool CommitOnEnter {
            get { return Storage.GetBoolean(CommitOnEnterKey, false); }
            set { WritableStorage?.SetBoolean(CommitOnEnterKey, value); }
        }


        public static bool ShowCompletionOnFirstChar {
            get { return Storage.GetBoolean(CompletionOnFirstCharKey, true); }
            set { WritableStorage?.SetBoolean(CompletionOnFirstCharKey, value); }
        }

        public static bool ShowCompletionOnTab {
            get { return Storage.GetBoolean(CompletionOnTabKey, false); }
            set { WritableStorage?.SetBoolean(CompletionOnTabKey, value); }
        }

        public static IndentType IndentType {
            get { return (IndentType)Storage.GetInteger(CommonSettings.FormatterIndentTypeKey, (int)IndentType.Spaces); }
            set { WritableStorage?.SetInteger(CommonSettings.FormatterIndentTypeKey, (int)value); }
        }

        public static int IndentSize {
            get { return Storage.GetInteger(CommonSettings.FormatterIndentSizeKey, 4); }
            set { WritableStorage?.SetInteger(CommonSettings.FormatterIndentSizeKey, value); }
        }

        public static IndentStyle IndentStyle {
            get { return (IndentStyle)Storage.GetInteger(CommonSettings.IndentStyleKey, (int)IndentStyle.Smart); }
            set { WritableStorage?.SetInteger(CommonSettings.IndentStyleKey, (int)value); }
        }

        public static int TabSize {
            get { return Storage.GetInteger(CommonSettings.FormatterTabSizeKey, 4); }
            set { WritableStorage?.SetInteger(CommonSettings.FormatterTabSizeKey, value); }
        }

        public static bool SendToReplOnCtrlEnter {
            get { return Storage.GetBoolean(REditorSettings.SendToReplOnCtrlEnterKey, true); }
            set { WritableStorage?.SetBoolean(REditorSettings.SendToReplOnCtrlEnterKey, value); }
        }

        public static bool SyntaxCheckInRepl {
            get { return Storage.GetBoolean(REditorSettings.SyntaxCheckInReplKey, false); }
            set { WritableStorage?.SetBoolean(REditorSettings.SyntaxCheckInReplKey, false); }
        }

        public static bool PartialArgumentNameMatch {
            get { return Storage.GetBoolean(REditorSettings.PartialArgumentNameMatchKey, false); }
            set { WritableStorage?.SetBoolean(REditorSettings.PartialArgumentNameMatchKey, value); }
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
