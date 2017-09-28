// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.LanguageServer.Settings {
    internal sealed class REditorSettings : IREditorSettings {
        private readonly IEditorSettingsStorage _storage;

        public REditorSettings() {
            _storage = new EditorSettingsStorage();
            LintOptions = new LintOptions(() => _storage);
        }

        public void Dispose() => _storage.Dispose();

        public event EventHandler<EventArgs> SettingsChanged;
        public bool AutoFormat { get; set; } = true;
        public bool CompletionEnabled { get; set; } = true;
        public int IndentSize { get; set; } = 2;
        public IndentType IndentType { get; set; } = IndentType.Spaces;
        public int TabSize { get; set; } = 2;
        public IndentStyle IndentStyle { get; set; } = IndentStyle.Smart;
        public bool SyntaxCheckEnabled { get; set; } = true;
        public bool SignatureHelpEnabled { get; set; } = true;
        public bool InsertMatchingBraces { get; set; } = true;
        public bool FormatOnPaste { get; set; }
        public bool FormatScope { get; set; }
        public bool CommitOnSpace { get; set; } = false;
        public bool CommitOnEnter { get; set; } = true;
        public bool ShowCompletionOnFirstChar { get; set; } = true;
        public bool ShowCompletionOnTab { get; set; } = true;
        public bool SyntaxCheckInRepl { get; set; }
        public bool PartialArgumentNameMatch { get; set; }
        public bool EnableOutlining { get; set; }
        public bool SmartIndentByArgument { get; set; } = true;
        public RFormatOptions FormatOptions { get; set; } = new RFormatOptions();
        public ILintOptions LintOptions { get; set; }
    }
}
