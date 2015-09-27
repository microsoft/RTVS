using System;
using System.IO;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.EditorFactory
{
    internal class EditorInstance : IEditorInstance
    {
        IEditorDocument _document;

        public EditorInstance(IWorkspaceItem workspaceItem, ITextBuffer diskBuffer, IEditorDocumentFactory documentFactory)
        {
            if (workspaceItem == null)
                throw new ArgumentNullException("workspaceItem");

            if (diskBuffer == null)
                throw new ArgumentNullException("diskBuffer");

            if (documentFactory == null)
                throw new ArgumentNullException("documentFactory");

            WorkspaceItem = workspaceItem;
            ViewBuffer = diskBuffer;

            _document = documentFactory.CreateDocument(this);

            ServiceManager.AddService<IEditorInstance>(this, ViewBuffer);
        }

        #region IEditorInstance
        public object WpfControl
        {
            get { return null; }
        }

        public IWorkspaceItem WorkspaceItem { get; private set; }
        public ITextBuffer ViewBuffer { get; private set; }

        public ICommandTarget GetCommandTarget(ITextView textView)
        {
            return RMainController.FromTextView(textView);
        }

        public string Caption
        {
            get { return Path.GetFileName(WorkspaceItem.Path); }
        }

        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (_document != null)
            {
                ServiceManager.RemoveService<IEditorInstance>(ViewBuffer);

                _document.Dispose();
                _document = null;
            }

            if (WorkspaceItem != null)
            {
                WorkspaceItem.Dispose();
                WorkspaceItem = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}