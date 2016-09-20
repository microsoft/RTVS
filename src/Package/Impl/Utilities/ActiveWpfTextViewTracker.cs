// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IActiveWpfTextViewTracker))]
    internal class ActiveWpfTextViewTracker : IActiveWpfTextViewTracker, IVsWindowFrameEvents {
        private readonly Dictionary<IContentType, IWpfTextView> _textViews;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private readonly IContentTypeRegistryService _registryService;

        public event EventHandler<ActiveTextViewChangedEventArgs> LastActiveTextViewChanged;

        [ImportingConstructor]
        public ActiveWpfTextViewTracker(IVsEditorAdaptersFactoryService editorAdaptersFactory, IContentTypeRegistryService registryService) {
            _editorAdaptersFactory = editorAdaptersFactory;
            _registryService = registryService;
            _textViews = new Dictionary<IContentType, IWpfTextView>();
        }

        public IWpfTextView LastActiveTextView { get; private set; }

        public IWpfTextView GetLastActiveTextView(IContentType contentType) {
            IWpfTextView value;
            return _textViews.TryGetValue(contentType, out value) ? value : null;
        }

        public IWpfTextView GetLastActiveTextView(string contentTypeName) {
            IContentType contentType = _registryService.GetContentType(contentTypeName);
            if (contentType != null) {
                IWpfTextView value;
                return _textViews.TryGetValue(contentType, out value) ? value : null;
            }
            return null;
        }

        public void OnFrameCreated(IVsWindowFrame frame) {
        }

        public void OnFrameDestroyed(IVsWindowFrame frame) {
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible) {
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen) {
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame) {
            var oldTextView = GetWpfTextView(oldFrame);
            if (oldTextView != null) {
                UpdateTextViewIfRequired(oldTextView);
            }

            var newTextView = GetWpfTextView(newFrame);
            if (newTextView != null) {
                UpdateTextViewIfRequired(newTextView);
            }

            LastActiveTextViewChanged?.Invoke(this, new ActiveTextViewChangedEventArgs(oldTextView, newTextView));
        }

        private void UpdateTextViewIfRequired(IWpfTextView wpfTextView) {
            LastActiveTextView = wpfTextView;

            var contentType = wpfTextView.TextBuffer.ContentType;
            _textViews[contentType] = wpfTextView;
        }

        private IWpfTextView GetWpfTextView(IVsWindowFrame frame) {
            IVsTextView textView = null;
            if (frame != null) {
                try {
                    textView = VsShellUtilities.GetTextView(frame);
                    // IDE occasionally throws 'not implemented' on close
                } catch (NotImplementedException) { }
            }
            return textView != null ? _editorAdaptersFactory.GetWpfTextView(textView) : null;
        }
    }
}