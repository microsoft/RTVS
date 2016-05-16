// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Main editor document for Markdown language
    /// </summary>
    public class MdEditorDocument : IEditorDocument {
        private readonly IContainedLanguageHandler _rLanguageHandler;
        private readonly IProjectionBufferManager _projectionBufferManager;

        #region Constructors
        public MdEditorDocument(ITextBuffer textBuffer, 
            IProjectionBufferFactoryService projectionBufferFactoryService, 
            IContentTypeRegistryService contentTypeRegistryService) {

            this.TextBuffer = textBuffer;
            ServiceManager.AddService<MdEditorDocument>(this, TextBuffer);

            _projectionBufferManager = new ProjectionBufferManager(textBuffer, projectionBufferFactoryService, contentTypeRegistryService, MdProjectionContentTypeDefinition.ContentType);
            _rLanguageHandler = new RLanguageHandler(textBuffer, _projectionBufferManager);
        }
        #endregion

        #region IEditorDocument
        public ITextBuffer TextBuffer { get; private set; }

#pragma warning disable 67
        public event EventHandler<EventArgs> DocumentClosing;
#pragma warning restore 67

        public virtual void Close() { }
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
        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                _projectionBufferManager?.Dispose();
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
