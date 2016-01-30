using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IActiveWpfTextViewTracker))]
    internal class ActiveWpfTextViewTracker : IActiveWpfTextViewTracker {
        private readonly Dictionary<IContentType, IWpfTextView> _textViews;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        [ImportingConstructor]
        public ActiveWpfTextViewTracker(IVsEditorAdaptersFactoryService editorAdaptersFactory) {
            _editorAdaptersFactory = editorAdaptersFactory;
            _textViews = new Dictionary<IContentType, IWpfTextView>();
        }

        public IWpfTextView GetLastActiveTextView(IContentType contentType) {
            IWpfTextView value;
            return _textViews.TryGetValue(contentType, out value) ? value : null;
        }
    }
}