// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.R.Editor.ContentType;
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
                var definitionNode = CodeNavigator.FindItemDefinition(_textBuffer, triggerPoint.Value, itemName);
                if (definitionNode != null) {
                    ITextDocument document = _textBuffer.GetTextDocument();
                    if (document != null) {
                        peekableItems.Add(new PeekItem(document.FilePath, definitionNode, itemName, _peekResultFactory));
                    }
                }
            }
        }

        public void Dispose() { }
    }
}
