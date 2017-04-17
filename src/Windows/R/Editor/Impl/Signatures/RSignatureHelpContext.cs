// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public sealed class RSignatureHelpContext: I {
        public int Position { get; set; }
        public ISignatureHelpSession Session { get; private set; }
        public ITextBuffer TextBuffer { get; private set; }
        public AstRoot AstRoot { get; private set; }
        public bool InternalFunctions { get; internal set; }

        public RSignatureHelpContext(ISignatureHelpSession session, ITextBuffer textBuffer, AstRoot ast, int position) {
            Session = session;
            TextBuffer = textBuffer;
            Position = position;
            AstRoot = ast;
        }
    }
}
