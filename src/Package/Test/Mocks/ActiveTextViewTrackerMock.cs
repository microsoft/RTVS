using System;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public sealed class ActiveTextViewTrackerMock : IActiveWpfTextViewTracker {
        private readonly WpfTextViewMock _textView;

        public ActiveTextViewTrackerMock(string content, string contentTypeName) {
            var tb = new TextBufferMock(content, contentTypeName);
            _textView = new WpfTextViewMock(tb);
        }

        public IWpfTextView GetLastActiveTextView(string contentType) {
            return _textView;
        }

        public event EventHandler<ActiveTextViewChangedEventArgs> LastActiveTextViewChanged;

        public IWpfTextView GetLastActiveTextView(IContentType contentType) {
            return _textView;
        }
    }
}
