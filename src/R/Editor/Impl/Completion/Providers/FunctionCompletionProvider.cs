using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.AST.Search;
using Microsoft.R.Editor.Completion.Definitions;
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

            // TODO: this is different in the console window where 
            // packages may have been loaded from the command line. 
            // We need an extensibility point here.
            IEnumerable<IPackageInfo> packages = GetPackages(context);

            // Get list of functions in the package
            foreach (IPackageInfo pkg in packages)
            {
                Debug.Assert(pkg != null);

                IEnumerable<INamedItemInfo> functions = pkg.Functions;
                if (functions != null)
                {
                    foreach (INamedItemInfo function in functions)
                    {
                        var completion = new RCompletion(function.Name, function.Name, function.Description, glyph);
                        completions.Add(completion);
                    }
                }
            }

            return completions;
        }
        #endregion

        private IEnumerable<IPackageInfo> GetPackages(RCompletionContext context)
        {
            if (context.IsInNameSpace())
            {
                return GetSpecificPackage(context);
            }

            return GetAllFilePackages(context);
        }

        private IEnumerable<IPackageInfo> GetSpecificPackage(RCompletionContext context)
        {
            List<IPackageInfo> packages = new List<IPackageInfo>();
            ITextSnapshot snapshot = context.Session.TextView.TextBuffer.CurrentSnapshot;
            int colons = 0;

            for (int i = context.Position - 1; i >= 0; i--, colons++)
            {
                char ch = snapshot[i];
                if (ch != ':')
                {
                    break;
                }
            }

            if (colons > 1 && colons < 4)
            {
                string packageName = string.Empty;
                int start = 0;
                int end = context.Position - colons;

                for (int i = end - 1; i >= 0; i--)
                {
                    char ch = snapshot[i];
                    if (char.IsWhiteSpace(ch))
                    {
                        start = i + 1;
                        break;
                    }
                }

                if (start < end)
                {
                    packageName = snapshot.GetText(Span.FromBounds(start, end));
                    if (packageName.Length > 0)
                    {
                        if (colons == 3)
                            context.InternalFunctions = true;

                        IPackageInfo package = PackageIndex.GetPackageByName(packageName);
                        if (package != null)
                        {
                            packages.Add(package);
                        }
                    }
                }
            }

            return packages;
        }

        private IEnumerable<IPackageInfo> GetAllFilePackages(RCompletionContext context)
        {
            List<IPackageInfo> packages = new List<IPackageInfo>();

            IEnumerable<string> filePackageNames = context.AstRoot.GetFilePackageNames();
            foreach (string packageName in filePackageNames)
            {
                IPackageInfo p = PackageIndex.GetPackageByName(packageName);
                // May be null if user mistyped package name in the library()
                // statement or package is not installed.
                if (p != null)
                {
                    packages.Add(p);
                }
            }

            IPackageInfo basePackage = PackageIndex.GetPackageByName("base");
            Debug.Assert(basePackage != null, "Base package information is missing");

            packages.Add(basePackage);

            return packages;
        }
    }
}
