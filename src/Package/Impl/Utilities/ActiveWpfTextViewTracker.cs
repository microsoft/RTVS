using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IActiveWpfTextViewTracker))]
    internal class ActiveWpfTextViewTracker : IActiveWpfTextViewTracker, IVsWindowFrameEvents {
        private readonly Dictionary<IContentType, IWpfTextView> _textViews;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private readonly IContentTypeRegistryService _registryService;

        [ImportingConstructor]
        public ActiveWpfTextViewTracker(IVsEditorAdaptersFactoryService editorAdaptersFactory, IContentTypeRegistryService registryService) {
            _editorAdaptersFactory = editorAdaptersFactory;
            _registryService = registryService;
            _textViews = new Dictionary<IContentType, IWpfTextView>();
        }

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
            var wpfTextView = GetWpfTextView(oldFrame);
            if (wpfTextView != null) {
                UpdateTextViewIfRequired(wpfTextView);
            }

            wpfTextView = GetWpfTextView(newFrame);
            if (wpfTextView != null) {
                UpdateTextViewIfRequired(wpfTextView);
            }
        }

        private void UpdateTextViewIfRequired(IWpfTextView wpfTextView) {
            var contentType = wpfTextView.TextBuffer.ContentType;
            _textViews[contentType] = wpfTextView;
        }

        private IWpfTextView GetWpfTextView(IVsWindowFrame frame) {
            if (frame == null) {
                return null;
            }

            var textView = VsShellUtilities.GetTextView(frame);
            if (textView == null) {
                return null;
            }

            return _editorAdaptersFactory.GetWpfTextView(textView);
        }
    }
}