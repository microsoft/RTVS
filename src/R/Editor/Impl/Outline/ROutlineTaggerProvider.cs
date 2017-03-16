// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Outline {
    /// <summary>
    /// Provider of the tagger (R code outliner in this context)
    /// for the Core VS text editor.
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class OutliningTaggerProvider : ITaggerProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public OutliningTaggerProvider(ICoreShell shell) {
            _shell = shell;
        }

        #region ITaggerProvider
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag {
            var tagger = ServiceManager.GetService<ROutliningTagger>(buffer);
            if (tagger == null) {
                var document = ServiceManager.GetService<REditorDocument>(buffer);
                if (document != null) {
                    tagger = new ROutliningTagger(document, _shell);
                }
            }
            return tagger as ITagger<T>;
        }
        #endregion
    }
}
