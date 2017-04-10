// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Main editor document for Markdown language
    /// </summary>
    public class MdEditorDocument : IMdEditorDocument {
        private readonly RLanguageHandler _rLanguageHandler;
        private readonly IProjectionBufferManager _projectionBufferManager;

        #region Constructors
        public MdEditorDocument(ITextBuffer textBuffer, IProjectionBufferFactoryService projectionBufferFactoryService, IContentTypeRegistryService contentTypeRegistryService, ICoreShell coreShell) {

            this.TextBuffer = textBuffer;
            ServiceManager.AddService(this, TextBuffer, coreShell);

            _projectionBufferManager = new ProjectionBufferManager(textBuffer, 
                        projectionBufferFactoryService, contentTypeRegistryService,
                        coreShell, MdProjectionContentTypeDefinition.ContentType, RContentTypeDefinition.ContentType);
            ContainedLanguageHandler = _rLanguageHandler = new RLanguageHandler(textBuffer, _projectionBufferManager, coreShell);
        }
        #endregion

        #region IEditorDocument
        public ITextBuffer TextBuffer { get; private set; }

        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        public string FilePath => TextBuffer.GetFilePath();

#pragma warning disable 67
        public event EventHandler<EventArgs> DocumentClosing;
#pragma warning restore 67

        public virtual void Close() { }
        #endregion

        #region IMdEditorDocument
        public IContainedLanguageHandler ContainedLanguageHandler { get; }
        #endregion

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IMdEditorDocument FromTextBuffer(ITextBuffer textBuffer) {
            var document = TryFromTextBuffer(textBuffer);
            Debug.Assert(document != null, "No editor document available");
            return document;
        }

        /// <summary>
        /// Retrieves document instance from text buffer
        /// </summary>
        public static IMdEditorDocument TryFromTextBuffer(ITextBuffer textBuffer) {
            return EditorExtensions.TryFromTextBuffer<IMdEditorDocument>(textBuffer, MdContentTypeDefinition.ContentType);
        }

        /// <summary>
        /// Given text view locates document in underlying text buffer graph.
        /// </summary>
        public static IMdEditorDocument FindInProjectedBuffers(ITextBuffer viewBuffer) {
            return EditorExtensions.FindInProjectedBuffers<IMdEditorDocument>(viewBuffer, MdContentTypeDefinition.ContentType);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if(disposing) {
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
