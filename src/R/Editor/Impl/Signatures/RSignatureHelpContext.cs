// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// R signature help context. Provides information about current document, 
    /// caret position and other necessary data for the function signature engine.
    /// </summary>
    public sealed class RSignatureHelpContext {
        public int Position { get; set; }
        public IEditorSignatureSession Session { get; }
        public IEditorBuffer EditorBuffer { get; }
        public AstRoot AstRoot { get; }
        public bool InternalFunctions { get; internal set; }

        public RSignatureHelpContext(IEditorSignatureSession session, IEditorBuffer editorBuffer, AstRoot ast, int position) {
            Session = session;
            EditorBuffer = editorBuffer;
            Position = position;
            AstRoot = ast;
        }
    }
}
