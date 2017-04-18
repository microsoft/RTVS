// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation;

namespace Microsoft.R.Editor.Document {
    /// <summary>
    /// Main editor document for the R language
    /// </summary>
    public class REditorDocument : IREditorDocument {
        private EditorTree _editorTree; // Editor parse tree (AST + dynamic tree update task)
        private TreeValidator _validator; // Asynchronous AST syntax checker

        protected EditorTree Tree {get;}

        #region Constructors
        public REditorDocument(IEditorBuffer editorBuffer, ICoreShell coreShell, IExpressionTermFilter termFilter = null) {
            EditorBuffer = editorBuffer;
            IsClosed = false;

            EditorBuffer.Services.AddService(this);
            EditorBuffer.Closing += OnBufferClosing;

            var tree = new EditorTree(EditorBuffer, coreShell, termFilter);
            tree.Build();
            EditorTree = tree;
        }
        #endregion

        #region IREditorDocument
        public IEditorBuffer EditorBuffer { get; private set; }
        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        public virtual string FilePath => EditorBuffer.FilePath;

#pragma warning disable 67
        public event EventHandler<EventArgs> Closing;
#pragma warning restore 67
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)=> Close();

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnBufferClosing(object sender, EventArgs e) => Close();
        #endregion

        #region IREditorDocument
        /// <summary>
        /// Editor parse tree (object model)
        /// </summary>
        public IREditorTree EditorTree { get; private set; }

        /// <summary>
        /// Closes the document
        /// </summary>
        public virtual void Close() {
            if (IsClosed) {
                return;
            }

            IsClosed = true;
            Closing?.Invoke(this, null);

            EditorTree?.Dispose(); // this will also remove event handlers
            EditorTree = null;

            if (Closing != null) {
                foreach (EventHandler<EventArgs> eh in Closing.GetInvocationList()) {
                    Debug.Fail(String.Format(CultureInfo.CurrentCulture, "There are still listeners in the EditorDocument.OnDocumentClosing event list: {0}", eh.Target));
                    Closing -= eh;
                }
            }

            EditorBuffer?.Services?.RemoveService(this);
            EditorBuffer = null;
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
        public virtual void BeginMassiveChange() { }

        /// <summary>
        /// Tells document that massive change to text buffer is complete. Document will perform full parse, 
        /// resume tracking of text buffer changes and classification (colorization).
        /// </summary>
        /// <returns>True if changes were made to the text buffer since call to BeginMassiveChange</returns>
        public virtual bool EndMassiveChange() => true;

        /// <summary>
        /// Indicates if massive change to the document is in progress. If massive change
        /// is in progress, tree updates and colorizer are suspended.
        /// </summary>
        public virtual bool IsMassiveChangeInProgress => false;

#pragma warning disable 67
        public event EventHandler<EventArgs> MassiveChangeBegun;
        public event EventHandler<EventArgs> MassiveChangeEnded;
#pragma warning restore 67
        #endregion

        protected void FireMassiveChangeBegun() => MassiveChangeBegun?.Invoke(this, EventArgs.Empty);
        protected void FireMassiveChangeEnded() => MassiveChangeEnded?.Invoke(this, EventArgs.Empty);
    }
}
