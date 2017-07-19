// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.Editor {
    public interface IREditorSettings: IEditorSettings {
        /// <summary>
        /// Format range on paste
        /// </summary>
        bool FormatOnPaste { get; }

        /// <summary>
        /// Format the entire scope upon '}'
        /// </summary>
        bool FormatScope { get; }

        /// <summary>
        /// Commit completion on Space ley
        /// </summary>
        bool CommitOnSpace { get; }

        /// <summary>
        /// Commit completion on Enter key
        /// </summary>
        bool CommitOnEnter { get; }

        /// <summary>
        /// Show completion on first character typed (C# style)
        /// </summary>
        bool ShowCompletionOnFirstChar { get; }

        /// <summary>
        /// Show completion on Tab key (RStudio style)
        /// </summary>
        bool ShowCompletionOnTab { get; }

        /// <summary>
        /// Enable syntax checking in REPL
        /// </summary>
        bool SyntaxCheckInRepl { get; }

        bool PartialArgumentNameMatch { get; }

        /// <summary>
        /// Enable code outlining (collapsible regions)
        /// </summary>
        bool EnableOutlining { get; }

        /// <summary>
        /// During smart indent align caret by function
        /// argument rather than indent one level deeper
        /// </summary>
        bool SmartIndentByArgument { get; }

        /// <summary>
        /// Formatter options
        /// </summary>
        RFormatOptions FormatOptions { get; }

        /// <summary>
        /// Lint checker options
        /// </summary>
        LintOptions LintOptions { get; }
    }
}
