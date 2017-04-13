// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.EditorFactory {
    public abstract class ProjectionEditorInstance : IEditorInstance {
        public ProjectionEditorInstance(ITextBuffer diskBuffer, ITextDocumentFactoryService textDocumentFactoryService, ICoreShell coreShell) {
            Check.ArgumentNull(nameof(diskBuffer), diskBuffer);
            Check.ArgumentNull(nameof(textDocumentFactoryService), textDocumentFactoryService);

            DiskBuffer = new EditorBuffer(diskBuffer, textDocumentFactoryService);

            var projectionBufferManager = ProjectionBufferManager.FromTextBuffer(diskBuffer);
            ViewBuffer = new EditorBuffer(projectionBufferManager.ViewBuffer, textDocumentFactoryService);

            DiskBuffer.Services.AddService(this);
            ViewBuffer.Services.AddService(this);
        }

        #region IEditorInstance
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
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        public abstract ICommandTarget GetCommandTarget(IEditorView editorView);
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            ViewBuffer.RemoveService<IEditorInstance>();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}