using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Completion.Engine;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Provider
{
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class FunctionCompletionProvider : IRCompletionListProvider
    {
        #region IRCompletionListProvider
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context)
        {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);

            string result = RCompletionEngine.HelpDataSource.GetHelpText("abs", "base").Result;

            completions.Add(new RCompletion("abs", "abs", string.Empty, glyph));

            return completions;
        }
        #endregion
    }
}
