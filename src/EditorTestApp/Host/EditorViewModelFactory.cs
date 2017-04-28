// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Application.Host {
    [ExcludeFromCodeCoverage]
    internal static class EditorViewModelFactory {
        public static IEditorViewModel CreateEditorViewModel(ITextBuffer textBuffer, IServiceContainer services) {
            var locator = services.GetService<IContentTypeServiceLocator>();
            var factory = locator.GetService<IEditorViewModelFactory>(textBuffer.ContentType.TypeName);
            return factory?.CreateEditorViewModel(textBuffer);
        }
    }
}
