using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Completion.Engine;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Main editor document for R language
    /// </summary>
    public class EditorDocument : IREditorDocument
    {
        [Import]
        private ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        #region IEditorDocument
        public ITextBuffer TextBuffer { get; private set; }

        [Import(AllowDefault = true)]
        public IWorkspace Workspace { get; set; }

        public IWorkspaceItem WorkspaceItem { get; private set; }

#pragma warning disable 67
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;
        public event EventHandler<EventArgs> DocumentClosing;
#pragma warning restore 67
        #endregion

        #region IREditorDocument
        public IEditorTree EditorTree
        {
            get { return _editorTree; }
        }

        public bool IsClosed { get; private set; }
        #endregion

        private EditorTree _editorTree;
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

            _editorTree = new EditorTree(textBuffer);
            //_validator = new TreeValidator(this.EditorTree);

            _editorTree.Build();

            RCompletionEngine.Initialize();
        }
        #endregion

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IREditorDocument FromTextBuffer(ITextBuffer textBuffer)
        {
            IREditorDocument document = TryFromTextBuffer(textBuffer);

            Debug.Assert(document != null, "No editor document available");
            return document;
        }

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IREditorDocument TryFromTextBuffer(ITextBuffer textBuffer)
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

            if (DocumentClosing != null)
                DocumentClosing(this, null);

            if (EditorTree != null)
            {
                _editorTree.Dispose(); // this will also remove event handlers
                _editorTree = null;
            }

            if (DocumentClosing != null)
            {
                foreach (EventHandler<EventArgs> eh in DocumentClosing.GetInvocationList())
                {
                    Debug.Fail(String.Format(CultureInfo.CurrentCulture, "There are still listeners in the EditorDocument.OnDocumentClosing event list: {0}", eh.Target));
                    DocumentClosing -= eh;
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
