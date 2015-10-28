using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Main editor document for Markdown language
    /// </summary>
    public class MdEditorDocument : IEditorDocument {
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

        public virtual void Close() { }
        #endregion

        #region Constructors
        public MdEditorDocument(ITextBuffer textBuffer, IWorkspaceItem workspaceItem) {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);

            this.TextBuffer = textBuffer;
            this.WorkspaceItem = workspaceItem;

            ServiceManager.AddService<MdEditorDocument>(this, TextBuffer);
        }
        #endregion

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IEditorDocument FromTextBuffer(ITextBuffer textBuffer) {
            IEditorDocument document = TryFromTextBuffer(textBuffer);
            Debug.Assert(document != null, "No editor document available");
            return document;
        }

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IEditorDocument TryFromTextBuffer(ITextBuffer textBuffer) {
            IEditorDocument document = ServiceManager.GetService<IEditorDocument>(textBuffer);
            if (document == null) {
                TextViewData viewData = TextViewConnectionListener.GetTextViewDataForBuffer(textBuffer);
                if (viewData != null && viewData.LastActiveView != null) {
                    MdMainController controller = MdMainController.FromTextView(viewData.LastActiveView);
                    if (controller != null && controller.TextBuffer != null) {
                        document = ServiceManager.GetService<MdEditorDocument>(controller.TextBuffer);
                    }
                }
            }

            return document;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) { }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
