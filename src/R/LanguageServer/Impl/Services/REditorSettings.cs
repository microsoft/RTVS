// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class REditorSettings : IREditorSettings {
        public void Dispose() {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> SettingsChanged;
        public bool AutoFormat { get; } = true;
        public bool CompletionEnabled { get; } = true;
        public int IndentSize { get; } = 2;
        public IndentType IndentType { get; } = IndentType.Spaces;
        public int TabSize { get; } = 2;
        public IndentStyle IndentStyle { get; } = IndentStyle.Smart;
        public bool SyntaxCheckEnabled { get; } = true;
        public bool SignatureHelpEnabled { get; } = true;
        public bool InsertMatchingBraces { get; } = true;
        public bool FormatOnPaste { get; }
        public bool FormatScope { get; }
        public bool CommitOnSpace { get; } = false;
        public bool CommitOnEnter { get; } = true;
        public bool ShowCompletionOnFirstChar { get; } = true;
        public bool ShowCompletionOnTab { get; } = true;
        public bool SendToReplOnCtrlEnter { get; }
        public bool SyntaxCheckInRepl { get; }
        public bool PartialArgumentNameMatch { get; }
        public bool EnableOutlining { get; }
        public RFormatOptions FormatOptions { get; } = new RFormatOptions();
        public LintOptions LintOptions { get; } = new LintOptions();
    }
}
