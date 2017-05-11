// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class PeekableItemSource : IPeekableItemSource {
        private readonly ITextBuffer _textBuffer;
        private readonly IPeekResultFactory _peekResultFactory;
        private readonly IServiceContainer _services;

        public PeekableItemSource(ITextBuffer textBuffer, IPeekResultFactory peekResultFactory, IServiceContainer services) {
            _textBuffer = textBuffer;
            _peekResultFactory = peekResultFactory;
            _services = services;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems) {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue) {
                return;
            }

            var itemName = session.TextView.GetIdentifierUnderCaret(out Span span);
            if (!string.IsNullOrEmpty(itemName)) {
                var textDocument = _textBuffer.GetTextDocument();
                var document = _textBuffer.GetEditorDocument<IREditorDocument>();
                var definitionNode = document?.EditorTree.AstRoot.FindItemDefinition(triggerPoint.Value, itemName);
                if (definitionNode != null) {
                    peekableItems.Add(new UserDefinedPeekItem(textDocument.FilePath, definitionNode, itemName, _peekResultFactory, _services));
                } else {
                    // Not found. Try internal functions
                    var item = new InternalFunctionPeekItem(textDocument.FilePath, span, itemName, _peekResultFactory, _services);
                    peekableItems.Add(item);
                }
            }
        }

        public void Dispose() { }
    }
}
