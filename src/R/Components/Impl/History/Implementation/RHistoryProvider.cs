// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History.Implementation {
    [Export(typeof(IRHistoryProvider))]
    internal class RHistoryProvider : IRHistoryProvider {
        private readonly ITextBufferFactoryService _textBufferFactory;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IRtfBuilderService _rtfBuilderService;
        private readonly ITextSearchService2 _textSearchService;
        private readonly IRSettings _settings;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly Dictionary<ITextBuffer, IRHistory> _histories;

        [ImportingConstructor]
        public RHistoryProvider(ITextBufferFactoryService textBufferFactory 
            , IContentTypeRegistryService contentTypeRegistryService
            , IEditorOperationsFactoryService editorOperationsFactory
            , IRtfBuilderService rtfBuilderService
            , ITextSearchService2 textSearchService
            , IRSettings settings) {
            _textBufferFactory = textBufferFactory;
            _editorOperationsFactory = editorOperationsFactory;
            _rtfBuilderService = rtfBuilderService;
            _textSearchService = textSearchService;
            _settings = settings;
            _rtfBuilderService = rtfBuilderService;
            _contentTypeRegistryService = contentTypeRegistryService;
            _histories = new Dictionary<ITextBuffer, IRHistory>();
        }

        public IRHistory GetAssociatedRHistory(ITextBuffer textBuffer) {
            IRHistory history;
            return _histories.TryGetValue(textBuffer, out history) ? history : null;
        }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            IRHistory history;
            return _histories.TryGetValue(textView.TextDataModel.DocumentBuffer, out history) ? history : null;
        }

        public IRHistoryFiltering CreateFiltering(IRHistoryWindowVisualComponent visualComponent) {
            var history = GetAssociatedRHistory(visualComponent.TextView);
            return new RHistoryFiltering(history, visualComponent, _settings, _textSearchService);
        }

        public IRHistory CreateRHistory(IRInteractiveWorkflow interactiveWorkflow) {
            var contentType = _contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
            var textBuffer = _textBufferFactory.CreateTextBuffer(contentType);
            var history = new RHistory(interactiveWorkflow, textBuffer, new FileSystem(), _settings, _editorOperationsFactory, _rtfBuilderService, () => RemoveRHistory(textBuffer));
            _histories[textBuffer] = history;
            return history;
        }

        private void RemoveRHistory(ITextBuffer textBuffer) {
            _histories.Remove(textBuffer);
        }
    }
}