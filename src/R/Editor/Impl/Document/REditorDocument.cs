using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Main editor document for R language
    /// </summary>
    public class REditorDocument : IEditorDocument
    {
        [Import(AllowDefault = true)]
        public IWorkspace Workspace { get; set; }

        #region Constructors
        public REditorDocument(ITextBuffer textBuffer, IWorkspaceItem workspaceItem)
        {
            this.TextBuffer = textBuffer;
            this.WorkspaceItem = workspaceItem;
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
        public static REditorDocument FromTextBuffer(ITextBuffer textBuffer)
        {
            REditorDocument document = TryFromTextBuffer(textBuffer);

            Debug.Assert(document != null, "No editor document available");
            return document;
        }

        public static REditorDocument TryFromTextBuffer(ITextBuffer textBuffer)
        {
            REditorDocument document = ServiceManager.GetService<REditorDocument>(textBuffer);
            if (document == null)
            {
                TextViewData viewData = TextViewConnectionListener.GetTextViewDataForBuffer(textBuffer);
                if (viewData != null && viewData.LastActiveView != null)
                {
                    RMainController controller = RMainController.FromTextView(viewData.LastActiveView);
                    if (controller != null && controller.TextBuffer != null)
                    {
                        document = ServiceManager.GetService<REditorDocument>(controller.TextBuffer);
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
