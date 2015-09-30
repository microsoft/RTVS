using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks
{
    [Export(typeof(IVsEditorAdaptersFactoryService))]
    public sealed class VsEditorAdaptersFactoryServiceMock : IVsEditorAdaptersFactoryService
    {
        public IVsCodeWindow CreateVsCodeWindowAdapter(OLE.Interop.IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(OLE.Interop.IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(OLE.Interop.IServiceProvider serviceProvider, IContentType contentType)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapterForSecondaryBuffer(OLE.Interop.IServiceProvider serviceProvider, ITextBuffer secondaryBuffer)
        {
            throw new NotImplementedException();
        }

        public IVsTextBufferCoordinator CreateVsTextBufferCoordinatorAdapter()
        {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(OLE.Interop.IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(OLE.Interop.IServiceProvider serviceProvider, ITextViewRoleSet roles)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer GetBufferAdapter(ITextBuffer textBuffer)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer GetDataBuffer(IVsTextBuffer bufferAdapter)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer GetDocumentBuffer(IVsTextBuffer bufferAdapter)
        {
            throw new NotImplementedException();
        }

        public IVsTextView GetViewAdapter(ITextView textView)
        {
            throw new NotImplementedException();
        }

        public IWpfTextView GetWpfTextView(IVsTextView viewAdapter)
        {
            throw new NotImplementedException();
        }

        public IWpfTextViewHost GetWpfTextViewHost(IVsTextView viewAdapter)
        {
            throw new NotImplementedException();
        }

        public void SetDataBuffer(IVsTextBuffer bufferAdapter, ITextBuffer dataBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
