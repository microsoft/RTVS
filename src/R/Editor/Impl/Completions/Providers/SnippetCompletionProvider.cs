// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// R language code snippets completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class SnippetCompletionProvider : IRCompletionListProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly ImageSource _snippetGlyph;

        [ImportingConstructor]
        public SnippetCompletionProvider([Import(AllowDefault = true)] ISnippetInformationSourceProvider snippetInformationSource, IGlyphService glyphService) {
            _snippetInformationSource = snippetInformationSource;
            _snippetGlyph = glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphCSharpExpansion, StandardGlyphItem.GlyphItemPublic);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            var infoSource = _snippetInformationSource?.InformationSource;

            if (!context.IsInNameSpace() && infoSource != null) {
                foreach (ISnippetInfo info in infoSource.Snippets) {
                    completions.Add(new RCompletion(info.Name, info.Name, info.Description, _snippetGlyph));
                }
            }
            return completions;
        }
        #endregion
    }
}
