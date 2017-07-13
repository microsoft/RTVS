// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Validation.Lint;

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

        public REditorSettings(ICoreShell coreShell) : base(coreShell, RContentTypeDefinition.ContentType) {
            CreateLintOptions();
        }

        public REditorSettings(IEditorSettingsStorage storage) : base(storage) {
            CreateLintOptions();
        }

        public override void ResetSettings() {
            _formatOptions = new RFormatOptions();
            CreateLintOptions();
            base.ResetSettings();
        }

        private void CreateLintOptions() {
            LintOptions = new LintOptions(() => Storage);
        }

        public bool FormatOnPaste {
            get => Storage.Get(FormatOnPasteKey, true);
            set => WritableStorage?.Set(FormatOnPasteKey, value);
        }

        public bool FormatScope {
            get => Storage.Get(FormatScopeKey, true);
            set => WritableStorage?.Set(FormatScopeKey, value);
        }

        public bool CommitOnSpace {
            get => Storage.Get(CommitOnSpaceKey, false);
            set => WritableStorage?.Set(CommitOnSpaceKey, value);
        }

        public bool CommitOnEnter {
            get => Storage.Get(CommitOnEnterKey, false);
            set => WritableStorage?.Set(CommitOnEnterKey, value);
        }

        public bool ShowCompletionOnFirstChar {
            get => Storage.Get(CompletionOnFirstCharKey, true);
            set => WritableStorage?.Set(CompletionOnFirstCharKey, value);
        }

        public bool ShowCompletionOnTab {
            get => Storage.Get(CompletionOnTabKey, false);
            set => WritableStorage?.Set(CompletionOnTabKey, value);
        }

        public bool SendToReplOnCtrlEnter {
            get => Storage.Get(SendToReplOnCtrlEnterKey, true);
            set => WritableStorage?.Set(SendToReplOnCtrlEnterKey, value);
        }

        public bool SyntaxCheckInRepl {
            get => Storage.Get(SyntaxCheckInReplKey, false);
            set => WritableStorage?.Set(SyntaxCheckInReplKey, value);
        }

        public bool PartialArgumentNameMatch {
            get => Storage.Get(PartialArgumentNameMatchKey, false);
            set => WritableStorage?.Set(PartialArgumentNameMatchKey, value);
        }

        public bool EnableOutlining {
            get => Storage.Get(EnableOutliningKey, true);
            set => WritableStorage?.Set(EnableOutliningKey, value);
        }

        public RFormatOptions FormatOptions {
            get {
                _formatOptions.IndentSize = IndentSize;
                _formatOptions.IndentType = IndentType;
                _formatOptions.TabSize = TabSize;
                return _formatOptions;
            }
        }

        public LintOptions LintOptions { get; private set; }
    }
}
