using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Provider
{
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class KeywordCompletionProvider : IRCompletionListProvider
    {
        #region IRCompletionListProvider
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context)
        {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);

            foreach (string keyword in Keywords.KeywordList)
            {
                completions.Add(new RCompletion(keyword, keyword, string.Empty, glyph));
            }

            return completions;
        }
        #endregion
    }
}
