using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Tree.Search;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion.Providers
{
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class FunctionCompletionProvider : IRCompletionListProvider
    {
        #region IRCompletionListProvider
        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context)
        {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);

            ITextBuffer textBiffer = context.Session.TextView.TextBuffer;
            EditorDocument document = EditorDocument.FromTextBuffer(textBiffer);

            // TODO: this is different in the console window where 
            // packages may have been loaded from the command line. 
            // We need an extensibility point here.

            IEnumerable<string> filePackageNames = document.EditorTree.AstRoot.GetFilePackageNames();

            List<IPackageInfo> filePackages = new List<IPackageInfo>();
            foreach(string packageName in filePackageNames)
            {
                IPackageInfo p = PackageIndex.GetPackageByName(packageName);
                // May be null if user mistyped package name in the library()
                // statement or package is not installed.
                if (p != null) 
                {
                    filePackages.Add(p);
                }
            }

            IPackageInfo basePackage = PackageIndex.GetPackageByName("base");
            Debug.Assert(basePackage != null, "Base package information is missing");

            filePackages.Add(basePackage);
            
            // Get list of functions in the package
            foreach (IPackageInfo pkg in filePackages)
            {
                Debug.Assert(pkg != null);

                IEnumerable<INamedItemInfo> functions = pkg.Functions;
                if (functions != null)
                {
                    foreach (INamedItemInfo function in functions)
                    {
                        Debug.Assert(function != null);

                        var completion = new RCompletion(function.Name, function.Name, function.Description, glyph);
                        completions.Add(completion);
                    }
                }
            }

            return completions;
        }
        #endregion
    }
}
