using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Classification;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Completion.Engine;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.R.Editor.Validation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Main editor document for R language
    /// </summary>
    public class REditorDocument : IREditorDocument
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

        private EditorTree _editorTree;
        private int _inMassiveChange;
        private TreeValidator _validator;

        #region Constructors
        public REditorDocument(ITextBuffer textBuffer, IWorkspaceItem workspaceItem)
        {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);

            this.TextBuffer = textBuffer;
            this.WorkspaceItem = workspaceItem;

            IsClosed = false;
            TextDocumentFactoryService.TextDocumentDisposed += OnTextDocumentDisposed;

            ServiceManager.AddService<REditorDocument>(this, TextBuffer);

            _editorTree = new EditorTree(textBuffer);
            if (!this.IsTransient)
            {
                _validator = new TreeValidator(this.EditorTree);
            }

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
            IREditorDocument document = ServiceManager.GetService<IREditorDocument>(textBuffer);
            if (document == null)
            {
                document = FindInProjectedBuffers(textBuffer);
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
            }

            return document;
        }

        public static IREditorDocument FindInProjectedBuffers(ITextBuffer viewBuffer)
        {
            IREditorDocument document = null;
            if (viewBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType))
            {
                return ServiceManager.GetService<REditorDocument>(viewBuffer);
            }

            // Try locating R buffer
            ITextBuffer rBuffer = null;
            IProjectionBuffer pb = viewBuffer as IProjectionBuffer;
            if (pb != null)
            {
                rBuffer = pb.SourceBuffers.FirstOrDefault((ITextBuffer tb) =>
                {
                    if (tb.ContentType.IsOfType(RContentTypeDefinition.ContentType))
                    {
                        document = ServiceManager.GetService<REditorDocument>(tb);
                        if(document != null)
                        {
                            return true;
                        }
                    }

                    return false;
                });
            }

            return document;
        }

        public static SnapshotPoint? MapCaretPositionFromView(ITextView textView)
        {
            int caretPosition = textView.Caret.Position.BufferPosition;
            var caretPoint = new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, caretPosition);
            return MapPointFromView(textView, caretPoint);
        }

        public static SnapshotPoint? MapPointFromView(ITextView textView, SnapshotPoint point)
        {
            IREditorDocument document = REditorDocument.FindInProjectedBuffers(textView.TextBuffer);
            ITextBuffer documentBuffer = document.TextBuffer;
            SnapshotPoint? documentPoint = textView.BufferGraph.MapDownToBuffer(point, PointTrackingMode.Positive, documentBuffer, PositionAffinity.Predecessor);
            return documentPoint;
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

        #region IREditorDocument
        /// <summary>
        /// Editor parse tree (object model)
        /// </summary>
        public IEditorTree EditorTree
        {
            get { return _editorTree; }
        }

        /// <summary>
        /// Closes the document
        /// </summary>
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

            ServiceManager.RemoveService<REditorDocument>(TextBuffer);
            TextBuffer = null;
        }

        /// <summary>
        /// If trie the document is closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Tells of document does not have associated disk file
        /// such as when document is based off projection buffer
        /// created elsewhere as in VS Interactive Window case.
        /// </summary>
        public bool IsTransient
        {
            get { return WorkspaceItem == null || WorkspaceItem.Path.Length == 0; }
        }

        /// <summary>
        /// Tells document that massive change to text buffer is about to commence.
        /// Document will then stop tracking text buffer changes, will suspend
        /// HTML parser anc classifier and remove all projections. HTML tree is
        /// no longer valid after this call.
        /// </summary>
        public void BeginMassiveChange()
        {
            if (_inMassiveChange == 0)
            {
                _editorTree.TreeUpdateTask.Suspend();

                RClassifier colorizer = ServiceManager.GetService<RClassifier>(TextBuffer);
                if (colorizer != null)
                    colorizer.Suspend();

                if (MassiveChangeBegun != null)
                    MassiveChangeBegun(this, EventArgs.Empty);
            }

            _inMassiveChange++;
        }

        /// <summary>
        /// Tells document that massive change to text buffer is complete. Document will perform full parse, 
        /// resume tracking of text buffer changes and classification (colorization).
        /// </summary>
        /// <returns>True if changes were made to the text buffer since call to BeginMassiveChange</returns>
        public bool EndMassiveChange()
        {
            bool changed = _editorTree.TreeUpdateTask.TextBufferChangedSinceSuspend;

            if (_inMassiveChange == 1)
            {
                RClassifier colorizer = ServiceManager.GetService<RClassifier>(TextBuffer);
                if (colorizer != null)
                    colorizer.Resume();

                if (changed)
                {
                    TextChangeEventArgs textChange = new TextChangeEventArgs(0, 0, TextBuffer.CurrentSnapshot.Length, 0,
                        new TextProvider(_editorTree.TextSnapshot, partial: true), new TextStream(string.Empty));

                    List<TextChangeEventArgs> textChanges = new List<TextChangeEventArgs>();
                    textChanges.Add(textChange);
                    _editorTree.FireOnUpdatesPending(textChanges);

                    _editorTree.FireOnUpdateBegin();
                    _editorTree.FireOnUpdateCompleted(TreeUpdateType.NewTree);
                }

                _editorTree.TreeUpdateTask.Resume();

                if (MassiveChangeEnded != null)
                    MassiveChangeEnded(this, EventArgs.Empty);
            }

            if (_inMassiveChange > 0)
                _inMassiveChange--;

            return changed;
        }

        /// <summary>
        /// Indicates if massive change to the document is in progress. If massive change
        /// is in progress, tree updates and colorizer are suspended.
        /// </summary>
        public bool IsMassiveChangeInProgress
        {
            get { return _inMassiveChange > 0; }
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> MassiveChangeBegun;
        public event EventHandler<EventArgs> MassiveChangeEnded;
#pragma warning restore 67
        #endregion
    }
}
