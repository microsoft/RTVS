using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers
{
    /// <summary>
    /// R language package name completion provider.
    /// </summary>
    public class PackagesCompletionProvider : IRCompletionListProvider
    {
        #region IRCompletionListProvider
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context)
        {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);

            IEnumerable<IPackageInfo> packages = PackageIndex.Packages;
            foreach (var packageInfo in packages)
            {
                completions.Add(new RCompletion(packageInfo.Name, packageInfo.Name, packageInfo.Description, glyph));
            }

            return completions;
        }
        #endregion
    }
}
