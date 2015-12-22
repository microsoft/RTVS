using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completions.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class KeywordCompletionProvider : IRCompletionListProvider {
        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();

            if (!context.IsInNameSpace()) {
                ImageSource keyWordGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);

                foreach (string keyword in Keywords.KeywordList) {
                    completions.Add(new RCompletion(keyword, keyword, string.Empty, keyWordGlyph));
                }

                ImageSource buildInGlyth = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupIntrinsic, StandardGlyphItem.GlyphItemPublic);
                foreach (string s in Builtins.BuiltinList) {
                    completions.Add(new RCompletion(s, s, string.Empty, buildInGlyth));
                }
            }

            return completions;
        }
        #endregion
    }
}
