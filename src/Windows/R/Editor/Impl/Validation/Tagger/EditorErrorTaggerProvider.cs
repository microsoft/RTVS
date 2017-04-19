// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.TaskList;
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
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            EditorErrorTagger tagger = null;

            if (document != null && TreeValidator.IsSyntaxCheckEnabled(textBuffer, _shell.GetService<IREditorSettings>())) {
                tagger = ServiceManager.GetService<EditorErrorTagger>(textBuffer);
                if (tagger == null) {
                    tagger = new EditorErrorTagger(textBuffer, _taskList, _shell);
                    ServiceManager.AddService(tagger, textBuffer, _shell);
                }
            }

            return tagger as ITagger<T>;
        }
    }
}
