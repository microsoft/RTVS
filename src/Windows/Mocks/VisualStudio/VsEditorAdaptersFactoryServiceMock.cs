// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IVsEditorAdaptersFactoryService))]
    public sealed class VsEditorAdaptersFactoryServiceMock : IVsEditorAdaptersFactoryService {
        private Dictionary<ITextBuffer, IVsTextBuffer> _textBufferAdapters = new Dictionary<ITextBuffer, IVsTextBuffer>();
        private Dictionary<IVsTextBuffer, ITextBuffer> _vsTextBufferAdapters = new Dictionary<IVsTextBuffer, ITextBuffer>();

        private Dictionary<ITextView, IVsTextView> _textViewAdapters = new Dictionary<ITextView, IVsTextView>();
        private Dictionary<IVsTextView, ITextView> _vsTextViewAdapters = new Dictionary<IVsTextView, ITextView>();

        public IVsCodeWindow CreateVsCodeWindowAdapter(OLE.Interop.IServiceProvider serviceProvider) {
            return new VsCodeWindowMock();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(OLE.Interop.IServiceProvider serviceProvider) {
            VsTextBufferMock tb = new VsTextBufferMock();
            _textBufferAdapters[tb.TextBuffer] = tb;
            _vsTextBufferAdapters[tb] = tb.TextBuffer;
            return tb;
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(OLE.Interop.IServiceProvider serviceProvider, IContentType contentType) {
            VsTextBufferMock tb = new VsTextBufferMock(contentType);
            _textBufferAdapters[tb.TextBuffer] = tb;
            _vsTextBufferAdapters[tb] = tb.TextBuffer;
            return tb;
        }

        public IVsTextBuffer CreateVsTextBufferAdapterForSecondaryBuffer(OLE.Interop.IServiceProvider serviceProvider, ITextBuffer secondaryBuffer) {
            VsTextBufferMock tb = new VsTextBufferMock(secondaryBuffer);
            _textBufferAdapters[tb.TextBuffer] = tb;
            _vsTextBufferAdapters[tb] = tb.TextBuffer;
            return tb;
        }

        public IVsTextBufferCoordinator CreateVsTextBufferCoordinatorAdapter() {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(OLE.Interop.IServiceProvider serviceProvider) {
            VsTextViewMock tv = new VsTextViewMock();
            _textViewAdapters[tv.TextView] = tv;
            _vsTextViewAdapters[tv] = tv.TextView;
            return tv;
        }

        public IVsTextView CreateVsTextViewAdapter(OLE.Interop.IServiceProvider serviceProvider, ITextViewRoleSet roles) {
            return CreateVsTextViewAdapter(serviceProvider);
        }

        public IVsTextBuffer GetBufferAdapter(ITextBuffer textBuffer) {
            IVsTextBuffer adapter = null;
            if(!_textBufferAdapters.TryGetValue(textBuffer, out adapter)) {
                adapter = CreateVsTextBufferAdapter(null, textBuffer.ContentType);
            }
            return adapter;
        }

        public ITextBuffer GetDataBuffer(IVsTextBuffer bufferAdapter) {
            ITextBuffer tb = null;
            _vsTextBufferAdapters.TryGetValue(bufferAdapter, out tb);
            return tb;
        }

        public ITextBuffer GetDocumentBuffer(IVsTextBuffer bufferAdapter) {
            return GetDataBuffer(bufferAdapter);
        }

        public IVsTextView GetViewAdapter(ITextView textView) {
            IVsTextView tv = null;
            if(!_textViewAdapters.TryGetValue(textView, out tv)) {
                tv = CreateVsTextViewAdapter(null);
            }
            return tv;
        }

        public IWpfTextView GetWpfTextView(IVsTextView viewAdapter) {
            ITextView tv = null;
            _vsTextViewAdapters.TryGetValue(viewAdapter, out tv);

            return tv as IWpfTextView;
        }

        public IWpfTextViewHost GetWpfTextViewHost(IVsTextView viewAdapter) {
            throw new NotImplementedException();
        }

        public void SetDataBuffer(IVsTextBuffer bufferAdapter, ITextBuffer dataBuffer) {
            IVsTextBuffer vsBuffer;
            if (_textBufferAdapters.TryGetValue(dataBuffer, out vsBuffer)) {
                _vsTextBufferAdapters.Remove(vsBuffer);
                _textBufferAdapters.Remove(dataBuffer);
            }

            ITextBuffer tb;
            if (_vsTextBufferAdapters.TryGetValue(bufferAdapter, out tb)) {
                _vsTextBufferAdapters.Remove(bufferAdapter);
                _textBufferAdapters.Remove(tb);
            }

            VsTextBufferMock mock = bufferAdapter as VsTextBufferMock;
            mock.TextBuffer = dataBuffer;

            _vsTextBufferAdapters[bufferAdapter] = dataBuffer;
            _textBufferAdapters[dataBuffer] = bufferAdapter;
        }
    }
}
