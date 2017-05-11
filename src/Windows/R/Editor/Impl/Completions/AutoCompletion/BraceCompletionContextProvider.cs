// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Completions.AutoCompletion {

    [Export(typeof(IBraceCompletionContextProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [BracePair('{', '}')]
    [BracePair('[', ']')]
    [BracePair('(', ')')]
    [BracePair('\'', '\'')]
    [BracePair('\"', '\"')]
    [BracePair('`', '`')]
    internal sealed class BraceCompletionContextProvider : IBraceCompletionContextProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public BraceCompletionContextProvider(ICoreShell shell) {
            _shell = shell;
        }

        /// <summary>
        /// Creates an <see cref="IBraceCompletionContext"/> to handle 
        /// language-specific actions such as parsing and formatting.
        /// </summary>
        /// <remarks>
        /// Opening points within strings and comments are usually invalid points to start 
        /// an <see cref="IBraceCompletionSession"/> and will return false.
        /// </remarks>
        /// <param name="textView">View containing the <paramref name="openingPoint"/>.</param>
        /// <param name="openingPoint">Insertion point of the <paramref name="openingBrace"/>.</param>
        /// <param name="openingBrace">Opening brace that has been typed by the user.</param>
        /// <param name="closingBrace">Closing brace character</param>
        /// <param name="context">Brace completion context if created.</param>
        /// <returns>Returns true if the <paramref name="openingPoint"/> 
        /// was a valid point in the buffer to start a <see cref="IBraceCompletionSession"/>.
        /// </returns>
        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context) {
            var document = openingPoint.Snapshot.TextBuffer.GetService<IREditorDocument>();
            if (document != null) {
                var ast = document.EditorTree.AstRoot;

                // We don't want to complete inside strings
                if (ast.IsPositionInsideString(openingPoint.Position)) {
                    context = null;
                    return false;
                }

                // We don't want to complete inside comments
                if (document.IsPositionInComment(openingPoint.Position)) {
                    context = null;
                    return false;
                }
            }
            context = new BraceCompletionContext(_shell);
            return true;
        }
    }
}
