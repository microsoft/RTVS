// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Document;

namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// Document factory 
    /// </summary>
    public interface IEditorDocumentFactory {
        /// <summary>
        /// Creates instance if editor document
        /// </summary>
        IEditorDocument CreateDocument(IEditorInstance editorInstance);
    }
}
