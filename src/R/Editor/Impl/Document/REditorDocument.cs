// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Classification;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.R.Editor.Document {
    /// <summary>
    /// Main editor document for the R language
    /// </summary>
    public class REditorDocument : IREditorDocument {
        private readonly ICoreShell _shell;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        #region IEditorDocument
        public ITextBuffer TextBuffer { get; private set; }

        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        public string FilePath {
            get {
                string path = null;
                var textView = this.GetFirstView();
                if (textView != null) {
                    if (textView.IsRepl()) {
                        return Resources.ReplWindowName;
                    }

                    if (textView.TextBuffer != TextBuffer) {
                        var pbm = ProjectionBufferManager.FromTextBuffer(textView.TextBuffer);
                        path = pbm?.DiskBuffer.GetFilePath();
                    } else {
                        path = TextBuffer.GetFilePath();
                    }
                }
                return path;
            }
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> DocumentClosing;
#pragma warning restore 67
        #endregion

        /// <summary>
        /// Editor parse tree (AST + dynamic tree update task)
        /// </summary>
        private EditorTree _editorTree;

        /// <summary>
        /// Counter of massive change requests. In massive changes
        /// such as when entire document is formatted parser is suspended
        /// in order to avoid multiple parse passes for every incremental
        /// change made to the document text buffer.
        /// </summary>
        private int _inMassiveChange;

        /// <summary>
        /// Asynchronous AST syntax checker
        /// </summary>
        private TreeValidator _validator;

        #region Constructors
        public REditorDocument(ITextBuffer textBuffer, ICoreShell shell) {
            _shell = shell;
            _textDocumentFactoryService = _shell.Services.GetService<ITextDocumentFactoryService>();
            _textDocumentFactoryService.TextDocumentDisposed += OnTextDocumentDisposed;

            TextBuffer = textBuffer;
            IsClosed = false;

            ServiceManager.AddService(this, TextBuffer, shell);
            var clh = ServiceManager.GetService<IContainedLanguageHost>(textBuffer);

            _editorTree = new EditorTree(textBuffer, shell, new ExpressionTermFilter(clh));
            if (REditorSettings.SyntaxCheckInRepl) {
                _validator = new TreeValidator(EditorTree, shell);
            }

            _editorTree.Build();
        }
        #endregion

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IREditorDocument FromTextBuffer(ITextBuffer textBuffer) {
            IREditorDocument document = TryFromTextBuffer(textBuffer);
            Debug.Assert(document != null, "No editor document available");
            return document;
        }

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IREditorDocument TryFromTextBuffer(ITextBuffer textBuffer) {
            return EditorExtensions.TryFromTextBuffer<IREditorDocument>(textBuffer, RContentTypeDefinition.ContentType);
        }

        /// <summary>
        /// Given text view locates R document in underlying text buffer graph.
        /// In REPL window there may be multiple R text buffers but usually
        /// only last one (the one active at the > prompt) has attached R document.
        /// Other R buffers represent previously typed commands. They still have
        /// colorizer attached but no active R documents.
        /// </summary>
        /// <param name="viewBuffer"></param>
        /// <returns></returns>
        public static IREditorDocument FindInProjectedBuffers(ITextBuffer viewBuffer) {
            return EditorExtensions.FindInProjectedBuffers<IREditorDocument>(viewBuffer, RContentTypeDefinition.ContentType);
        }

        /// <summary>
        /// Locates first R buffer in the projection buffer graph.
        /// Note that in REPL this may not be the active buffer.
        /// In REPL used <see cref="FindInProjectedBuffers"/>.
        /// </summary>
        /// <param name="viewBuffer"></param>
        /// <returns></returns>
        public static ITextBuffer FindRBuffer(ITextBuffer viewBuffer) {
            if (viewBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                return viewBuffer;
            }

            // Try locating R buffer
            ITextBuffer rBuffer = null;
            IProjectionBuffer pb = viewBuffer as IProjectionBuffer;
            if (pb != null) {
                rBuffer = pb.SourceBuffers.FirstOrDefault((ITextBuffer tb) => {
                    return tb.ContentType.IsOfType(RContentTypeDefinition.ContentType);
                });
            }

            return rBuffer;
        }

        /// <summary>
        /// Maps caret position in text view to position in the projected 
        /// R editor text buffer. R text buffer can be projected into view
        /// in REPL window case or in case when R is embedded in another
        /// language file such as in SQL file.
        /// </summary>
        /// <param name="textView"></param>
        /// <returns></returns>
        public static SnapshotPoint? MapCaretPositionFromView(ITextView textView) {
            try {
                SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
                return MapPointFromView(textView, caretPosition);
            } catch (ArgumentException) { }
            return null;
        }

        /// <summary>
        /// Maps given point from view buffer to R editor buffer
        /// </summary>
        public static SnapshotPoint? MapPointFromView(ITextView textView, SnapshotPoint point) {
            var pb = textView.TextBuffer as IProjectionBuffer;
            if (pb != null) {
                return pb.MapDown(point, RContentTypeDefinition.ContentType);
            }
            return point;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            Close();
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        void OnTextDocumentDisposed(object sender, TextDocumentEventArgs e) {
            if (e.TextDocument.TextBuffer == this.TextBuffer) {
                Close();
            }
        }
        #endregion

        #region IREditorDocument
        /// <summary>
        /// Editor parse tree (object model)
        /// </summary>
        public IEditorTree EditorTree => _editorTree;

        /// <summary>
        /// Closes the document
        /// </summary>
        public virtual void Close() {
            if (IsClosed) {
                return;
            }

            IsClosed = true;
            _textDocumentFactoryService.TextDocumentDisposed -= OnTextDocumentDisposed;

            DocumentClosing?.Invoke(this, null);

            if (EditorTree != null) {
                _editorTree.Dispose(); // this will also remove event handlers
                _editorTree = null;
            }

            if (DocumentClosing != null) {
                foreach (EventHandler<EventArgs> eh in DocumentClosing.GetInvocationList()) {
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
        /// Tells document that massive change to text buffer is about to commence.
        /// Document will then stop tracking text buffer changes, will suspend
        /// R parser anc classifier and remove all projections. AST is no longer 
        /// valid after this call.
        /// </summary>
        public void BeginMassiveChange() {
            if (_inMassiveChange == 0) {
                _editorTree.TreeUpdateTask.Suspend();

                RClassifier colorizer = ServiceManager.GetService<RClassifier>(TextBuffer);
                colorizer?.Suspend();

                MassiveChangeBegun?.Invoke(this, EventArgs.Empty);
            }

            _inMassiveChange++;
        }

        /// <summary>
        /// Tells document that massive change to text buffer is complete. Document will perform full parse, 
        /// resume tracking of text buffer changes and classification (colorization).
        /// </summary>
        /// <returns>True if changes were made to the text buffer since call to BeginMassiveChange</returns>
        public bool EndMassiveChange() {
            bool changed = _editorTree.TreeUpdateTask.TextBufferChangedSinceSuspend;

            if (_inMassiveChange == 1) {
                var colorizer = ServiceManager.GetService<RClassifier>(TextBuffer);
                colorizer?.Resume();

                if (changed) {
                    TextChangeEventArgs textChange =
                        new TextChangeEventArgs(0, 0, TextBuffer.CurrentSnapshot.Length, 0,
                            new TextProvider(_editorTree.TextSnapshot, partial: true),
                            new TextStream(string.Empty));

                    List<TextChangeEventArgs> textChanges = new List<TextChangeEventArgs>();
                    textChanges.Add(textChange);
                    _editorTree.FireOnUpdatesPending(textChanges);
                }

                _editorTree.TreeUpdateTask.Resume();
                MassiveChangeEnded?.Invoke(this, EventArgs.Empty);
            }

            if (_inMassiveChange > 0) {
                _inMassiveChange--;
            }

            return changed;
        }

        /// <summary>
        /// Indicates if massive change to the document is in progress. If massive change
        /// is in progress, tree updates and colorizer are suspended.
        /// </summary>
        public bool IsMassiveChangeInProgress => _inMassiveChange > 0;

#pragma warning disable 67
        public event EventHandler<EventArgs> MassiveChangeBegun;
        public event EventHandler<EventArgs> MassiveChangeEnded;
#pragma warning restore 67
        #endregion

        private class ExpressionTermFilter : IExpressionTermFilter {
            private readonly IContainedLanguageHost _clh;
            public ExpressionTermFilter(IContainedLanguageHost clh) {
                _clh = clh;
            }
            public bool IsInertRange(ITextRange range) {
                return _clh != null ? _clh.IsInertRange(range) : false;
            }
        }
    }
}
