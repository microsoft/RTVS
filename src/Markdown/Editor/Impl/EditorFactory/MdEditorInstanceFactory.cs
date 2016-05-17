// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.EditorFactory {
    /// <summary>
    /// Editor instance factory. Typically imported via MEF
    /// in the host application editor factory such as in
    /// IVsEditorFactory.CreateEditorInstance.
    /// </summary>
    [Export(typeof(IEditorFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class MdEditorInstanceFactory : IEditorFactory {
        public IEditorInstance CreateEditorInstance(ITextBuffer textBuffer, IEditorDocumentFactory documentFactory) {
            if (textBuffer == null) {
                throw new ArgumentNullException(nameof(textBuffer));
            }
            if (documentFactory == null) {
                throw new ArgumentNullException(nameof(documentFactory));
            }
            return new MdEditorInstance(textBuffer, documentFactory);
        }
    }
}
