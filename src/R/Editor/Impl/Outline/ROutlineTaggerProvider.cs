// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Outline
{
    /// <summary>
    /// Provider or tagger (code outliner in this context)
    /// for the core VS text editor.
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class OutliningTaggerProvider : ITaggerProvider
    {
        #region ITaggerProvider
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var tagger = ServiceManager.GetService<ROutliningTagger>(buffer);
            if (tagger == null)
            {
                var document = ServiceManager.GetService<REditorDocument>(buffer);
                if (document != null)
                    tagger = new ROutliningTagger(document);
            }
            return tagger as ITagger<T>;
        }
        #endregion
    }
}
