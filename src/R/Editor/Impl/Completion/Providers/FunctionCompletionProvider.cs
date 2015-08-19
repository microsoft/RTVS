using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree.Search;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
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

            IEnumerable<IPackageInfo> filePackages = document.EditorTree.AstRoot.GetFilePackages();
            IEnumerable<IPackageInfo> allPackages = filePackages.Union(PackageIndex.BasePackages);
            
            // Get list of functions in the package
            foreach (IPackageInfo pkg in allPackages)
            {
                IEnumerable<string> functionNames = pkg.Functions;
                if (functionNames != null)
                {
                    foreach (string functionName in functionNames)
                    {
                        string description = FunctionIndex.GetFunctionDescription(functionName);

                        var completion = new RCompletion(functionName, functionName, description, glyph);
                        completions.Add(completion);
                    }
                }
            }

            return completions;
        }
        #endregion
    }
}
