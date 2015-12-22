using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completions.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    public class PackagesCompletionProvider : IRCompletionListProvider {
        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);

            IEnumerable<IPackageInfo> packages = PackageIndex.Packages;
            foreach (var packageInfo in packages) {
                completions.Add(new RCompletion(packageInfo.Name, packageInfo.Name, packageInfo.Description, glyph));
            }

            return completions;
        }
        #endregion
    }
}
