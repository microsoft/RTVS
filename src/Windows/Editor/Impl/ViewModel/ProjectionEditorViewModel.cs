// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.ViewModel {
    public abstract class ProjectionEditorViewModel : EditorViewModel {
        protected ProjectionEditorViewModel(IEditorDocument document, ITextDocumentFactoryService textDocumentFactoryService) : 
            base(document, CreateViewBuffer(document.TextBuffer(), textDocumentFactoryService)) { }

        private static IEditorBuffer CreateViewBuffer(ITextBuffer diskBuffer, ITextDocumentFactoryService textDocumentFactoryService) {
            var projectionBufferManager = ProjectionBufferManager.FromTextBuffer(diskBuffer);
            return new EditorBuffer(projectionBufferManager.ViewBuffer, textDocumentFactoryService);
        }
    }
}