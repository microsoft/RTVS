// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.EditorFactory {
    internal class EditorInstance : IEditorInstance {
        IEditorDocument _document;

        public EditorInstance(ITextBuffer diskBuffer, IEditorDocumentFactory documentFactory) {
            if (diskBuffer == null)
                throw new ArgumentNullException(nameof(diskBuffer));

            if (documentFactory == null)
                throw new ArgumentNullException(nameof(documentFactory));

             ViewBuffer = diskBuffer;
            _document = documentFactory.CreateDocument(this);

            ServiceManager.AddService<IEditorInstance>(this, ViewBuffer);
        }

        #region IEditorInstance
        /// <summary>
        /// Text buffer containing document data that is 
        /// to be attached to a text view. 
        /// </summary>
        public ITextBuffer ViewBuffer { get; private set; }

        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        public ICommandTarget GetCommandTarget(ITextView textView) {
            return MdMainController.FromTextView(textView);
        }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (_document != null) {
                ServiceManager.RemoveService<IEditorInstance>(ViewBuffer);

                _document.Dispose();
                _document = null;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}