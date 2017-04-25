// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Application.Host {
    [ExcludeFromCodeCoverage]
    internal static class EditorViewModelFactory {
        public static IEditorViewModel CreateEditorViewModel(ITextBuffer textBuffer, ICompositionService compositionService) {
            var importComposer = new ContentTypeImportComposer<IEditorViewModelFactory>(compositionService);
            var factory = importComposer.GetImport(textBuffer.ContentType.TypeName);
            return factory?.CreateEditorViewModel(textBuffer.ToEditorBuffer());
        }
    }
}
