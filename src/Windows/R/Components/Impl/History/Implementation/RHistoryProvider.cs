// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History.Implementation {
    internal class RHistoryProvider : IRHistoryProvider {
        private readonly ITextBufferFactoryService _textBufferFactory;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IRtfBuilderService _rtfBuilderService;
        private readonly ITextSearchService2 _textSearchService;
        private readonly IRSettings _settings;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly Dictionary<ITextBuffer, IRHistoryVisual> _histories;
        private readonly IFileSystem _fs;

        public RHistoryProvider(IServiceContainer services) {
            _textBufferFactory = services.GetService<ITextBufferFactoryService>();
            _contentTypeRegistryService = services.GetService<IContentTypeRegistryService>();
            _editorOperationsFactory = services.GetService<IEditorOperationsFactoryService>();
            _rtfBuilderService = services.GetService<IRtfBuilderService>();
            _textSearchService = services.GetService<ITextSearchService2>();
            _settings = services.GetService<IRSettings>();
            _histories = new Dictionary<ITextBuffer, IRHistoryVisual>();
            _fs = services.FileSystem();
        }

        public IRHistoryVisual GetAssociatedRHistory(ITextBuffer textBuffer) 
            => _histories.TryGetValue(textBuffer, out IRHistoryVisual history) ? history : null;

        public IRHistoryVisual GetAssociatedRHistory(ITextView textView) 
            => _histories.TryGetValue(textView.TextDataModel.DocumentBuffer, out IRHistoryVisual history) ? history : null;

        public IRHistoryFiltering CreateFiltering(IRHistoryWindowVisualComponent visualComponent) {
            var history = GetAssociatedRHistory(visualComponent.TextView);
            return new RHistoryFiltering(history, visualComponent, _settings, _textSearchService);
        }

        public IRHistory CreateRHistory(IRInteractiveWorkflowVisual interactiveWorkflow) {
            var contentType = _contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
            var textBuffer = _textBufferFactory.CreateTextBuffer(contentType);
            var history = new RHistory(interactiveWorkflow, textBuffer, _fs, _settings, _editorOperationsFactory, _rtfBuilderService, () => RemoveRHistory(textBuffer));
            _histories[textBuffer] = history;
            return history;
        }

        private void RemoveRHistory(ITextBuffer textBuffer) {
            _histories.Remove(textBuffer);
        }
    }
}