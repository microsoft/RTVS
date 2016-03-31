// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class PeekableItemSource : IPeekableItemSource {
        private readonly ITextBuffer _textBuffer;
        private readonly IPeekResultFactory _peekResultFactory;

        public PeekableItemSource(ITextBuffer textBuffer, IPeekResultFactory peekResultFactory) {
            _textBuffer = textBuffer;
            _peekResultFactory = peekResultFactory;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems) {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            Span span;
            string itemName = session.TextView.GetIdentifierUnderCaret(out span);
            if (!string.IsNullOrEmpty(itemName)) {
                var document = REditorDocument.FromTextBuffer(_textBuffer);
                var definitionNode = document.EditorTree.AstRoot.FindItemDefinition(triggerPoint.Value, itemName);
                if (definitionNode != null) {
                    ITextDocument textDocument = _textBuffer.GetTextDocument();
                    if (textDocument != null) {
                        peekableItems.Add(new PeekItem(textDocument.FilePath, definitionNode, itemName, _peekResultFactory));
                    }
                }
            }
        }

        public void Dispose() { }
    }
}
