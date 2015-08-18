using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Packages;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers
{
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    public class PackagesCompletionProvider : IRCompletionListProvider
    {
        #region IRCompletionListProvider
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context)
        {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);

            IEnumerable<PackageInfo> basePackages = PackagesDataSource.GetBasePackages(fetchDescriptions: true);
            foreach (var packageInfo in basePackages)
            {
                completions.Add(new RCompletion(packageInfo.Name, packageInfo.Name, packageInfo, glyph));
            }

            IEnumerable<IPackageInfo> userPackages = PackagesDataSource.GetUserPackages(fetchDescriptions: true);
            foreach (var packageInfo in userPackages)
            {
                completions.Add(new RCompletion(packageInfo.Name, packageInfo.Name, packageInfo, glyph));
            }

            return completions;
        }
        #endregion
    }
}
