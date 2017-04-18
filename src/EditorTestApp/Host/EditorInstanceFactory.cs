// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Composition;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Application.Host {
    [ExcludeFromCodeCoverage]
    internal class EditorInstanceFactory {
        public static IEditorInstance CreateEditorInstance(ITextBuffer textBuffer, ICompositionService compositionService) {
            var importComposer = new ContentTypeImportComposer<IEditorFactory>(compositionService);
            var factory = importComposer.GetImport(textBuffer.ContentType.TypeName);

            var documentFactoryImportComposer = new ContentTypeImportComposer<IEditorDocumentFactory>(compositionService);
            var documentFactory = documentFactoryImportComposer.GetImport(textBuffer.ContentType.TypeName);

             return factory?.CreateEditorInstance(textBuffer, documentFactory);
        }
    }
}
