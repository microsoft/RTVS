using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Main editor document for R language
    /// </summary>
    public class EditorDocument : IEditorDocument
    {
        [Import]
        private ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import(AllowDefault = true)]
        public IWorkspace Workspace { get; set; }

        public EditorTree EditorTree { get; private set; }

        public bool IsClosed { get; private set; }

        #region IEditorDocument
        public ITextBuffer TextBuffer { get; private set; }

        public IWorkspaceItem WorkspaceItem { get; private set; }

#pragma warning disable 67
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;
        public event EventHandler<EventArgs> OnDocumentClosing;
#pragma warning restore 67
        #endregion

        //private TreeValidator _validator;

        #region Constructors
        public EditorDocument(ITextBuffer textBuffer, IWorkspaceItem workspaceItem)
        {
            EditorShell.CompositionService.SatisfyImportsOnce(this);

            this.TextBuffer = textBuffer;
            this.WorkspaceItem = workspaceItem;

            IsClosed = false;
            TextDocumentFactoryService.TextDocumentDisposed += OnTextDocumentDisposed;

            ServiceManager.AddService<EditorDocument>(this, TextBuffer);

            this.EditorTree = new EditorTree(textBuffer);
            //_validator = new TreeValidator(this.EditorTree);

            this.EditorTree.Build();
        }
        #endregion

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static EditorDocument FromTextBuffer(ITextBuffer textBuffer)
        {
            EditorDocument document = TryFromTextBuffer(textBuffer);

            Debug.Assert(document != null, "No editor document available");
            return document;
        }

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static EditorDocument TryFromTextBuffer(ITextBuffer textBuffer)
        {
            EditorDocument document = ServiceManager.GetService<EditorDocument>(textBuffer);
            if (document == null)
            {
                TextViewData viewData = TextViewConnectionListener.GetTextViewDataForBuffer(textBuffer);
                if (viewData != null && viewData.LastActiveView != null)
                {
                    RMainController controller = RMainController.FromTextView(viewData.LastActiveView);
                    if (controller != null && controller.TextBuffer != null)
                    {
                        document = ServiceManager.GetService<EditorDocument>(controller.TextBuffer);
                    }
                }
            }

            return document;
        }

        public virtual void Close()
        {
            if (IsClosed)
                return;

            IsClosed = true;

            TextDocumentFactoryService.TextDocumentDisposed -= OnTextDocumentDisposed;

            if (OnDocumentClosing != null)
                OnDocumentClosing(this, null);

            if (EditorTree != null)
            {
                EditorTree.Dispose(); // this will also remove event handlers
                EditorTree = null;
            }

            if (OnDocumentClosing != null)
            {
                foreach (EventHandler<EventArgs> eh in OnDocumentClosing.GetInvocationList())
                {
                    Debug.Fail(String.Format(CultureInfo.CurrentCulture, "There are still listeners in the EditorDocument.OnDocumentClosing event list: {0}", eh.Target));
                    OnDocumentClosing -= eh;
                }
            }

            ServiceManager.RemoveService<EditorDocument>(TextBuffer);
            TextBuffer = null;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        void OnTextDocumentDisposed(object sender, TextDocumentEventArgs e)
        {
            if (e.TextDocument.TextBuffer == this.TextBuffer)
            {
                Close();
            }
        }
        #endregion
    }
}
