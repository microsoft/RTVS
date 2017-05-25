// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.ViewModel {
    /// <summary>
    /// Represents instance of the editor to the host application
    /// </summary>
    public abstract class EditorViewModel : IEditorViewModel {
        private IEditorDocument _document;

        protected EditorViewModel(IEditorDocument document, IEditorBuffer viewBuffer = null) {
            Check.ArgumentNull(nameof(document), document);

            DiskBuffer = document.EditorBuffer;
            DiskBuffer.AddService(this);

            ViewBuffer = viewBuffer ?? DiskBuffer;
            if (viewBuffer != null) {
                ViewBuffer.AddService(this);
            }

            _document = document;
        }

        #region IEditorViewModel
        /// <summary>
        /// Text buffer containing document data that is to be attached to the text view. 
        /// In languages that support projected language scenarios this is the top level
        /// projection buffer. In regular scenarios the same as the disk buffer.
        /// </summary>
        public IEditorBuffer ViewBuffer { get; }

        /// <summary>
        /// Buffer that contains original content as it was retrieved from disk
        /// or generated in memory. 
        /// </summary>
        public IEditorBuffer DiskBuffer { get; }

        /// <summary>
        /// Retreives editor document
        /// </summary>
        public T GetDocument<T>() where T : class, IEditorDocument => _document as T;

        /// <summary>
        /// Retrieves editor command target (controller) for a particular view
        /// </summary>
        public abstract ICommandTarget GetCommandTarget(IEditorView editorView);
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            ViewBuffer?.RemoveService(this);
            DiskBuffer?.RemoveService(this);
            _document?.Dispose();
            _document = null;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}