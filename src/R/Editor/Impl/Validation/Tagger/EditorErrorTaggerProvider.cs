// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Validation.Tagger
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TagType(typeof(ErrorTag))]
    internal sealed class EditorErrorTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer textBuffer) where T : ITag
        {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            EditorErrorTagger tagger = null;

            if (document != null && TreeValidator.IsSyntaxCheckEnabled(textBuffer))
            {
                tagger = ServiceManager.GetService<EditorErrorTagger>(textBuffer);
                if (tagger == null)
                    tagger = new EditorErrorTagger(textBuffer);
            }

            return tagger as ITagger<T>;
        }
    }
}
