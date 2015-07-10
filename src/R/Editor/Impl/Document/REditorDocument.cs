using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Main editor document for R language
    /// </summary>
    public class EditorDocument : IEditorDocument
    {
        [Import(AllowDefault = true)]
        public IWorkspace Workspace { get; set; }

        public EditorTree EditorTree { get; private set; }

        #region Constructors
        public EditorDocument(ITextBuffer textBuffer, IWorkspaceItem workspaceItem)
        {
            this.TextBuffer = textBuffer;
            this.WorkspaceItem = workspaceItem;
            this.EditorTree = new EditorTree(textBuffer);
        }
        #endregion

        #region IEditorDocument
        public ITextBuffer TextBuffer { get; private set; }

        public IWorkspaceItem WorkspaceItem { get; private set; }

#pragma warning disable 67
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;
        public event EventHandler<EventArgs> OnDocumentClosing;
#pragma warning restore 67
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
        }
    }
}
