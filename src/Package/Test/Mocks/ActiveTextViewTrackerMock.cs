// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public sealed class ActiveTextViewTrackerMock : IActiveWpfTextViewTracker {
        private readonly WpfTextViewMock _textView;

        public ActiveTextViewTrackerMock(string content, string contentTypeName) {
            var tb = new TextBufferMock(content, contentTypeName);
            _textView = new WpfTextViewMock(tb);
        }

        public IWpfTextView LastActiveTextView => _textView;

        public IWpfTextView GetLastActiveTextView(string contentType) {
            return _textView;
        }
#pragma warning disable 67
        public event EventHandler<ActiveTextViewChangedEventArgs> LastActiveTextViewChanged;

        public IWpfTextView GetLastActiveTextView(IContentType contentType) {
            return _textView;
        }
    }
}
