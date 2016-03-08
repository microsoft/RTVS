// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class KeywordCompletionProvider : IRCompletionListProvider {
        [Import(AllowDefault = true)]
        private ISnippetInformationSourceProvider SnippetInformationSource { get; set; }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();

            if (!context.IsInNameSpace()) {
                var infoSource = SnippetInformationSource?.InformationSource;
                ImageSource keyWordGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);

                foreach (string keyword in Keywords.KeywordList) {
                    bool? isSnippet = infoSource?.IsSnippet(keyword);
                    if (!isSnippet.HasValue || !isSnippet.Value) {
                        completions.Add(new RCompletion(keyword, keyword, string.Empty, keyWordGlyph));
                    }
                }

                ImageSource buildInGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupIntrinsic, StandardGlyphItem.GlyphItemPublic);
                foreach (string s in Builtins.BuiltinList) {
                    completions.Add(new RCompletion(s, s, string.Empty, buildInGlyph));
                }
            }

            return completions;
        }
        #endregion
    }
}
