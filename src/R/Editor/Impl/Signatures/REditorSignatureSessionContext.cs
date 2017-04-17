// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public sealed class REditorSignatureSessionContext {
        public int Position { get; set; }
        public IEditorSignatureSession Session { get; private set; }
        public IEditorBuffer EditorBuffer { get; private set; }
        public AstRoot AstRoot { get; private set; }
        public bool InternalFunctions { get; internal set; }

        public REditorSignatureSessionContext(IEditorSignatureSession session, IEditorBuffer editorBuffer, AstRoot ast, int position) {
            Session = session;
            EditorBuffer = editorBuffer;
            Position = position;
            AstRoot = ast;
        }
    }
}
