// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public sealed class RCompletionContext: CompletionContext, IRCompletionContext {
        public AstRoot AstRoot { get; private set; }
        public bool InternalFunctions { get; internal set; }

        public RCompletionContext(IEditorCompletionSession session, IEditorBuffer editorBuffer, AstRoot ast, int position) : 
            base(session, editorBuffer, position) {
             AstRoot = ast;
        }
    }
}
