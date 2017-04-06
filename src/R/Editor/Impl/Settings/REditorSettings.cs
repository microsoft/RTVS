// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Core.Formatting;

namespace Microsoft.R.Editor.Settings {
    public sealed class REditorSettings: EditorSettings, IWritableREditorSettings, IREditorSettings {
        public const string FormatScopeKey = "FormatScope";
        public const string FormatOnPasteKey = "FormatOnPaste";
        public const string CommitOnSpaceKey = "CommitOnSpace";
        public const string CommitOnEnterKey = "CommitOnEnter";
        public const string CompletionOnFirstCharKey = "CompletionOnFirstChar";
        public const string CompletionOnTabKey = "CompletionOnTab";
        public const string SendToReplOnCtrlEnterKey = "SendToReplOnCtrlEnter";
        public const string SyntaxCheckInReplKey = "SyntaxCheckInRepl";
        public const string PartialArgumentNameMatchKey = "PartialArgumentNameMatch";
        public const string EnableOutliningKey = "EnableOutlining";
        public const string ShowInternalFunctionsKey = "ShowInternalFunctions";
        public const string ShowTclFunctionsKey = "ShowTclFunctions";

        private RFormatOptions _formatOptions = new RFormatOptions();

        public REditorSettings(IEditorSettingsStorage storage): base(storage) { }

        public override void ResetSettings() {
            _formatOptions = new RFormatOptions();
            base.ResetSettings();
        }

        public bool FormatOnPaste {
            get { return Storage.Get(FormatOnPasteKey, true); }
            set { WritableStorage?.Set(FormatOnPasteKey, value); }
        }

        public bool FormatScope {
            get { return Storage.Get(FormatScopeKey, true); }
            set { WritableStorage?.Set(FormatScopeKey, value); }
        }

        public bool CommitOnSpace {
            get { return Storage.Get(CommitOnSpaceKey, true); }
            set { WritableStorage?.Set(CommitOnSpaceKey, value); }
        }

        public bool CommitOnEnter {
            get { return Storage.Get(CommitOnEnterKey, true); }
            set { WritableStorage?.Set(CommitOnEnterKey, value); }
        }

        public bool ShowCompletionOnFirstChar {
            get { return Storage.Get(CompletionOnFirstCharKey, true); }
            set { WritableStorage?.Set(CompletionOnFirstCharKey, value); }
        }

        public bool ShowCompletionOnTab {
            get { return Storage.Get(CompletionOnTabKey, true); }
            set { WritableStorage?.Set(CompletionOnTabKey, value); }
        }

        public bool SendToReplOnCtrlEnter {
            get { return Storage.Get(SendToReplOnCtrlEnterKey, true); }
            set { WritableStorage?.Set(SendToReplOnCtrlEnterKey, value); }
        }

        public bool SyntaxCheckInRepl {
            get { return Storage.Get(SyntaxCheckInReplKey, true); }
            set { WritableStorage?.Set(SyntaxCheckInReplKey, value); }
        }

        public bool PartialArgumentNameMatch {
            get { return Storage.Get(PartialArgumentNameMatchKey, true); }
            set { WritableStorage?.Set(PartialArgumentNameMatchKey, value); }
        }

        public bool EnableOutlining {
            get { return Storage.Get(EnableOutliningKey, true); }
            set { WritableStorage?.Set(EnableOutliningKey, value); }
        }

        public RFormatOptions FormatOptions {
            get {
                _formatOptions.IndentSize = IndentSize;
                _formatOptions.IndentType = IndentType;
                _formatOptions.TabSize = TabSize;
                return _formatOptions;
            }
        }
    }
}
