// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.EditorFactory {
    /// <summary>
    /// Represents instance of the editor to the host application
    /// </summary>
    internal class EditorInstance : IEditorInstance {
        IEditorDocument _document;

        public EditorInstance(ITextBuffer diskBuffer, IEditorDocumentFactory documentFactory) {
            if (diskBuffer == null) {
                throw new ArgumentNullException(nameof(diskBuffer));
            }
            if (documentFactory == null) {
                throw new ArgumentNullException(nameof(documentFactory));
            }

            ViewBuffer = diskBuffer;
            _document = documentFactory.CreateDocument(this);

            ServiceManager.AddService<IEditorInstance>(this, ViewBuffer);
        }

        #region IEditorInstance
        public ITextBuffer ViewBuffer { get; }

        public ITextBuffer ProjectedBuffer => null;

        public ICommandTarget GetCommandTarget(ITextView textView) {
            return RMainController.FromTextView(textView);
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