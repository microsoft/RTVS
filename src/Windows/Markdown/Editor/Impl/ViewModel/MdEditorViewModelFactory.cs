// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ViewModel {
    /// <summary>
    /// Editor instance factory. Typically imported via MEF
    /// in the host application editor factory such as in
    /// IVsEditorFactory.CreateEditorInstance.
    /// </summary>
    [Export(typeof(IEditorViewModelFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class MdEditorViewModelFactory : IEditorViewModelFactory {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public MdEditorViewModelFactory(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public IEditorViewModel CreateEditorViewModel(ITextBuffer textBuffer) {
            Check.ArgumentNull(nameof(textBuffer), textBuffer);
            return new MdEditorViewModel(textBuffer, _coreShell.Services);
        }
    }
}
