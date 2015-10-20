using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers
{
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public sealed class WorkspaceVaraibleCompletionProvider : IRCompletionListProvider
    {
        [Import]
        IVariablesProvider _variablesProvider = null;

        #region IRCompletionListProvider
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context)
        {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource functionGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
            ImageSource variableGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);

            // Get list of functions in the package
            foreach (INamedItemInfo v in _variablesProvider.Variables)
            {
                Debug.Assert(v != null);

                ImageSource glyph = v.ItemType == NamedItemType.Variable ? variableGlyph : functionGlyph;
                var completion = new RCompletion(v.Name, v.Name, v.Description, glyph);
                completions.Add(completion);
            }

            return completions;
        }
        #endregion
    }
}
