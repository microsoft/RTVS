// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.Editor {
    public interface IREditorSettings: IEditorSettings {
        bool FormatOnPaste { get; }
        bool FormatScope { get; }
        bool CommitOnSpace { get; }
        bool CommitOnEnter { get; }
        bool ShowCompletionOnFirstChar { get; }
        bool ShowCompletionOnTab { get; }
        bool SendToReplOnCtrlEnter { get; }
        bool SyntaxCheckInRepl { get; }
        bool PartialArgumentNameMatch { get; }
        bool EnableOutlining { get; }
        RFormatOptions FormatOptions { get; }
        LintOptions LintOptions { get; }
    }
}
