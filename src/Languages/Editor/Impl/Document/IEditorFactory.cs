// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// Editor instance factory. Typically imported via MEF
    /// in the host application editor factory such as in
    /// IVsEditorFactory.CreateEditorInstance.
    /// </summary>
    public interface IEditorFactory {
        /// <summary>
        /// Creates an instance of an editor over the text buffer.
        /// </summary>
        IEditorInstance CreateEditorInstance(IEditorBuffer textBuffer, IEditorDocumentFactory documentFactory);
    }
}
