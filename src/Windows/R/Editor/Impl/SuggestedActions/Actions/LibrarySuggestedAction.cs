// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    internal abstract class LibrarySuggestedAction : RSuggestedActionBase {
        protected LibrarySuggestedAction(ITextView textView, ITextBuffer textBuffer, IRInteractiveWorkflow workflow, int position, string displayText) :
            base(textView, textBuffer, workflow, position, displayText) {

        }

        public override void Invoke(CancellationToken cancellationToken) {
            var document = TextBuffer.GetEditorDocument<IREditorDocument>();
            var tree = document.EditorTree;
            if (tree.IsReady) {
                var libraryName = document.EditorTree.AstRoot.IsInLibraryStatement(TextView.Caret.Position.BufferPosition);
                if (!string.IsNullOrEmpty(libraryName)) {
                    SubmitToInteractive(GetCommand(libraryName) + "\n", cancellationToken);
                }
            }
        }

        protected abstract string GetCommand(string libraryName);
    }
}
