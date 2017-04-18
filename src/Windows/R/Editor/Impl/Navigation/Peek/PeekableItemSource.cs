// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class PeekableItemSource : IPeekableItemSource {
        private readonly ITextBuffer _textBuffer;
        private readonly IPeekResultFactory _peekResultFactory;
        private readonly ICoreShell _shell;

        public PeekableItemSource(ITextBuffer textBuffer, IPeekResultFactory peekResultFactory, ICoreShell shell) {
            _textBuffer = textBuffer;
            _peekResultFactory = peekResultFactory;
            _shell = shell;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems) {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            Span span;
            string itemName = session.TextView.GetIdentifierUnderCaret(out span);
            if (!string.IsNullOrEmpty(itemName)) {
                ITextDocument textDocument = _textBuffer.GetTextDocument();
                var document = REditorDocument.TryFromTextBuffer(_textBuffer);
                var definitionNode = document?.EditorTree.AstRoot.FindItemDefinition(triggerPoint.Value, itemName);
                if (definitionNode != null) {
                    peekableItems.Add(new UserDefinedPeekItem(textDocument.FilePath, definitionNode, itemName, _peekResultFactory, _shell));
                } else {
                    // Not found. Try internal functions
                    IPeekableItem item = new InternalFunctionPeekItem(textDocument.FilePath, span, itemName, _peekResultFactory, _shell);
                    if (item != null) {
                        peekableItems.Add(item);
                    }
                }
            }
        }

        public void Dispose() { }
    }
}
