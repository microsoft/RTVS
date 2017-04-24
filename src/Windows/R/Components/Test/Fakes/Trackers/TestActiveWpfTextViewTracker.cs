// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Test.Fakes.Trackers {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IActiveWpfTextViewTracker))]
    [Export(typeof(TestActiveWpfTextViewTracker))]
    [PartMetadata(PartMetadataAttributeNames.SkipInEditorTestCompositionCatalog, null)]
    internal sealed class TestActiveWpfTextViewTracker : IActiveWpfTextViewTracker {
        private readonly Dictionary<IContentType, IWpfTextView> _textViews;
        private readonly IContentTypeRegistryService _registryService;

        [ImportingConstructor]
        public TestActiveWpfTextViewTracker(IContentTypeRegistryService registryService) {
            _textViews = new Dictionary<IContentType, IWpfTextView>();
            _registryService = registryService;
        }

        public void SetLastActiveTextView(IWpfTextView wpfTextView) {
            var contentType = wpfTextView.TextBuffer.ContentType;
            IWpfTextView oldValue;
            if (_textViews.TryGetValue(contentType, out oldValue) && oldValue.Equals(wpfTextView)) {
                return;
            }

            _textViews[contentType] = LastActiveTextView = wpfTextView;
            LastActiveTextViewChanged?.Invoke(this, new ActiveTextViewChangedEventArgs(oldValue, wpfTextView));
        }

        public IWpfTextView LastActiveTextView { get; private set; }

        public IWpfTextView GetLastActiveTextView(IContentType contentType) {
            IWpfTextView value;
            return _textViews.TryGetValue(contentType, out value) ? value : null;
        }

        public IWpfTextView GetLastActiveTextView(string contentTypeName) {
            IContentType contentType = _registryService.GetContentType(contentTypeName);
            if (contentType == null) {
                return null;
            }
            return GetLastActiveTextView(contentType);
        }

        public event EventHandler<ActiveTextViewChangedEventArgs> LastActiveTextViewChanged;
    }
}