// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.Editor {
    public interface IWritableREditorSettings : IWritableEditorSettings {
        bool FormatOnPaste { get; set; }
        bool FormatScope { get; set; }
        bool CommitOnSpace { get; set; }
        bool CommitOnEnter { get; set; }
        bool ShowCompletionOnFirstChar { get; set; }
        bool ShowCompletionOnTab { get; set; }
        bool SendToReplOnCtrlEnter { get; set; }
        bool SyntaxCheckInRepl { get; set; }
        bool PartialArgumentNameMatch { get; set; }
        bool EnableOutlining { get; set; }
        RFormatOptions FormatOptions { get; }
        LintOptions LintOptions { get; }
    }
}
