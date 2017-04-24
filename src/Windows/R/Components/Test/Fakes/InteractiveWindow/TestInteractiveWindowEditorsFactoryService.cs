// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Test.Fakes.InteractiveWindow {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IInteractiveWindowEditorFactoryService))]
    internal sealed class TestInteractiveWindowEditorsFactoryService : IInteractiveWindowEditorFactoryService {
        private const string ContentType = "text";

        private readonly ITextBufferFactoryService _textBufferFactoryService;
        private readonly ITextEditorFactoryService _textEditorFactoryService;
        private readonly IContentTypeRegistryService _contentTypeRegistry;

        [ImportingConstructor]
        public TestInteractiveWindowEditorsFactoryService(ITextBufferFactoryService textBufferFactoryService, ITextEditorFactoryService textEditorFactoryService, IContentTypeRegistryService contentTypeRegistry) {
            _textBufferFactoryService = textBufferFactoryService;
            _textEditorFactoryService = textEditorFactoryService;
            _contentTypeRegistry = contentTypeRegistry;
        }

        public IWpfTextView CreateTextView(IInteractiveWindow window, ITextBuffer buffer, ITextViewRoleSet roles) {
            var textView = _textEditorFactoryService.CreateTextView(buffer, roles);
            return _textEditorFactoryService.CreateTextViewHost(textView, false).TextView;
        }

        ITextBuffer IInteractiveWindowEditorFactoryService.CreateAndActivateBuffer(IInteractiveWindow window) {
            IContentType contentType;
            if (!window.Properties.TryGetProperty(typeof(IContentType), out contentType)) {
                contentType = _contentTypeRegistry.GetContentType(ContentType);
            }

            return _textBufferFactoryService.CreateTextBuffer(contentType);
        }
    }
}