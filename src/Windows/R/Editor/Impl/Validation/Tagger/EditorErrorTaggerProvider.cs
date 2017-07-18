// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Validation.Tagger {
    [Export(typeof(ITaggerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TagType(typeof(ErrorTag))]
    internal sealed class EditorErrorTaggerProvider : ITaggerProvider {
        private readonly ICoreShell _shell;
        private readonly IEditorTaskList _taskList;

        [ImportingConstructor]
        public EditorErrorTaggerProvider(ICoreShell shell, [Import(AllowDefault = true)] IEditorTaskList taskList) {
            _shell = shell;
            _taskList = taskList;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer textBuffer) where T : ITag {
            var document = textBuffer.GetEditorDocument<IREditorDocument>();
            if(document != null && TreeValidator.IsSyntaxCheckEnabled(textBuffer.ToEditorBuffer(), _shell.GetService<IREditorSettings>(), out var unused1, out var unused2)) {
                return textBuffer.Properties.GetOrCreateSingletonProperty(() => new EditorErrorTagger(textBuffer, _taskList, _shell.Services)) as ITagger<T>;
            }
            return null;
        }
    }
}
