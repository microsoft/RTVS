// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// Represents instance of the editor to the host application
    /// </summary>
    public abstract class EditorInstance : IEditorInstance {
        IEditorDocument _document;

        public EditorInstance(ITextBuffer diskBuffer, IEditorDocumentFactory documentFactory, ICoreShell coreShell, bool projected) {
            if (diskBuffer == null) {
                throw new ArgumentNullException(nameof(diskBuffer));
            }
            if (documentFactory == null) {
                throw new ArgumentNullException(nameof(documentFactory));
            }

            IsProjected = projected;
            ViewBuffer = DiskBuffer = diskBuffer;
            _document = documentFactory.CreateDocument(this);

            ServiceManager.AddService<IEditorInstance>(this, ViewBuffer, coreShell);
        }

        #region IEditorInstance
        /// <summary>
        /// Text buffer containing document data that is to be attached to the text view. 
        /// In languages that support projected language scenarios this is the top level
        /// projection buffer. In regular scenarios the same as the disk buffer.
        /// </summary>
        public ITextBuffer ViewBuffer { get; }

        /// <summary>
        /// Buffer that contains original content as it was retrieved from disk
        /// or generated in memory. 
        /// </summary>
        public ITextBuffer DiskBuffer { get; }

        public abstract ICommandTarget GetCommandTarget(ITextView textView);

        /// <summary>
        /// Indicates if document is projected into a view of another document.
        /// </summary>
        public bool IsProjected { get; }
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