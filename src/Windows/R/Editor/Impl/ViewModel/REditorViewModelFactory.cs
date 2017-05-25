// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ViewModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.EditorFactory {
    [Export(typeof(IEditorViewModelFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class REditorViewModelFactory : IEditorViewModelFactory {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public REditorViewModelFactory(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public IEditorViewModel CreateEditorViewModel(ITextBuffer textBuffer) {
            Check.ArgumentNull(nameof(textBuffer), textBuffer);
            return new REditorViewModel(textBuffer, _coreShell.Services);
        }
    }
}
