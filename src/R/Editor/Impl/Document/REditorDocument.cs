// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation;
using static System.FormattableString;

namespace Microsoft.R.Editor.Document {
    /// <summary>
    /// Main editor document for the R language
    /// </summary>
    public sealed class REditorDocument : IREditorDocument {
        private readonly IServiceContainer _services;
        private readonly TreeValidator _validator;

        public REditorDocument(IEditorBuffer editorBuffer, IServiceContainer services, bool isRepl) {
            EditorBuffer = editorBuffer;
            IsRepl = isRepl;

            _services = services;

            EditorBuffer.Services.AddService(this);
            EditorBuffer.Closing += OnBufferClosing;

            var tree = new EditorTree(EditorBuffer, services);
            tree.Build();
            EditorTree = tree;

            _validator = new TreeValidator(EditorTree, services);
        }

        #region IREditorDocument
        public IEditorBuffer EditorBuffer { get; private set; }
        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        public string FilePath => EditorBuffer.FilePath;

#pragma warning disable 67
        public event EventHandler<EventArgs> Closing;
#pragma warning restore 67
        #endregion

        #region IDisposable
        public void Dispose() => Close();

        private void OnBufferClosing(object sender, EventArgs e) => Close();
        #endregion

        #region IREditorDocument
        /// <summary>
        /// Editor parse tree (object model)
        /// </summary>
        public IREditorTree EditorTree { get; private set; }

        /// <summary>
        /// Document represents content in the interactive window
        /// </summary>
        public bool IsRepl { get; }

        /// <summary>
        /// Closes the document
        /// </summary>
        public void Close() {
            if (IsClosed) {
                return;
            }

            IsClosed = true;
            Closing?.Invoke(this, null);

            EditorTree?.Dispose(); // this will also remove event handlers
            EditorTree = null;

            if (Closing != null) {
                foreach (var eh in Closing.GetInvocationList()) {
                    var closingHandler = eh as EventHandler<EventArgs>;
                    if (closingHandler != null) {
                        Debug.Fail(Invariant($"There are still listeners in the EditorDocument.OnDocumentClosing event list: {eh.Target}"));
                        Closing -= closingHandler;
                    }
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
        public void BeginMassiveChange() { }

        /// <summary>
        /// Tells document that massive change to text buffer is complete. Document will perform full parse, 
        /// resume tracking of text buffer changes and classification (colorization).
        /// </summary>
        /// <returns>True if changes were made to the text buffer since call to BeginMassiveChange</returns>
        public bool EndMassiveChange() => true;

        /// <summary>
        /// Indicates if massive change to the document is in progress. If massive change
        /// is in progress, tree updates and colorizer are suspended.
        /// </summary>
        public bool IsMassiveChangeInProgress => false;

        public IEditorView PrimaryView => _services.GetService<IEditorViewLocator>()?.GetPrimaryView(EditorBuffer);

#pragma warning disable 67
        public event EventHandler<EventArgs> MassiveChangeBegun;
        public event EventHandler<EventArgs> MassiveChangeEnded;
#pragma warning restore 67
        #endregion

        public void FireMassiveChangeBegun() => MassiveChangeBegun?.Invoke(this, EventArgs.Empty);
        public void FireMassiveChangeEnded() => MassiveChangeEnded?.Invoke(this, EventArgs.Empty);
    }
}
