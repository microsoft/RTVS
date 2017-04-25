// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Main editor document for Markdown language
    /// </summary>
    public class MdEditorDocument : IMdEditorDocument {
        private readonly RLanguageHandler _rLanguageHandler;
        private readonly IProjectionBufferManager _projectionBufferManager;

        #region Constructors
        public MdEditorDocument(IEditorBuffer editorBuffer, IServiceContainer services) {
            EditorBuffer = editorBuffer;
            EditorBuffer.AddService(this);

            _projectionBufferManager = new ProjectionBufferManager(editorBuffer.As<ITextBuffer>(), services, MdProjectionContentTypeDefinition.ContentType, RContentTypeDefinition.ContentType);
            ContainedLanguageHandler = _rLanguageHandler = new RLanguageHandler(textBuffer, _projectionBufferManager, services);
        }
        #endregion

        #region IEditorDocument
        public IEditorBuffer EditorBuffer { get; }

        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        public string FilePath => EditorBuffer.FilePath;
        public bool IsClosed { get; private set; }
#pragma warning disable 67
        public event EventHandler<EventArgs> Closing;
#pragma warning restore 67

        public virtual void Close() => IsClosed = true;
        #endregion

        #region IMdEditorDocument
        public IContainedLanguageHandler ContainedLanguageHandler { get; }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                EditorBuffer.RemoveService(this);
                _projectionBufferManager.Dispose();
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
