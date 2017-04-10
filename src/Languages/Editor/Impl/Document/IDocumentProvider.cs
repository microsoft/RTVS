// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor {
    /// <summary>
    /// Provides ability to locate document from view or text buffer
    /// </summary>
    public interface IDocumentProvider {
        IEditorDocument GetDocument(IEditorBuffer editorBuffer);
        IEditorDocument GetDocument(IEditorView view);
    }
}
