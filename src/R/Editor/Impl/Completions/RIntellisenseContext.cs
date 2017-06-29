// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public sealed class RIntellisenseContext : IntellisenseContext, IRIntellisenseContext {
        public AstRoot AstRoot { get; }
        public bool InternalFunctions { get; set; }
        public bool AutoShownCompletion { get; }
        public bool IsRHistoryRequest { get; }

        public RIntellisenseContext(IEditorIntellisenseSession session, IEditorBuffer editorBuffer, AstRoot ast, int position, bool autoShown = true, bool isRHistoryRequest = false) : 
            base(session, editorBuffer, position) {
            AstRoot = ast;
            AutoShownCompletion = autoShown;
            IsRHistoryRequest = isRHistoryRequest;
        }
    }
}
