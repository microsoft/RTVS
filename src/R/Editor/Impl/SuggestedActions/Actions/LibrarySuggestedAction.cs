using System;
using System.Threading;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    internal abstract class LibrarySuggestedAction : RSuggestedActionBase {
        public LibrarySuggestedAction(ITextView textView, ITextBuffer textBuffer, int position, string displayText) :
            base(textView, textBuffer, position, displayText) {

        }

        public override void Invoke(CancellationToken cancellationToken) {
            var document = REditorDocument.FromTextBuffer(TextBuffer);
            var tree = document.EditorTree;
            if (tree.IsReady) {
                string libraryName = document.EditorTree.AstRoot.IsInLibraryStatement(TextView.Caret.Position.BufferPosition);
                if (!string.IsNullOrEmpty(libraryName)) {
                    SubmitToInteractive(GetCommand(libraryName) + "\n", cancellationToken);
                }
            }
        }

        protected abstract string GetCommand(string libraryName);
    }
}
