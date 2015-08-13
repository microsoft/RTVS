using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Support.Packages;
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

            // TODO: this needs to be an extensibility point
            IEnumerable<PackageInfo> packages = GetAvailablePackages(document.EditorTree.AstRoot);

            // Get list of functions in the package
            foreach (PackageInfo pkg in packages)
            {
                IEnumerable<FunctionInfo> functions = pkg.Functions;
                if (functions != null)
                {
                    foreach (FunctionInfo func in functions)
                    {
                        var completion = new RCompletion(func.Name, func.Name, func.Description, glyph);
                        completions.Add(completion);
                    }
                }
            }

            return completions;
        }
        #endregion

        /// <summary>
        /// Retrieves list of packages available to the current file.
        /// Consists of packages in the base library and packages
        /// added via 'library' statements.
        /// </summary>
        private IEnumerable<PackageInfo> GetAvailablePackages(AstRoot ast)
        {
            // TODO: this is different in the console window where 
            // packages may have been loaded from the command line. 
            // We need an extensibility point here.

            AstLibrarySearch search = new AstLibrarySearch();
            ast.Accept(search, null);

            IEnumerable<PackageInfo> basePackages = InstalledPackages.GetBasePackages();
            search.Packages.AddRange(basePackages);

            return search.Packages;
        }

        private class AstLibrarySearch : IAstVisitor
        {
            public List<PackageInfo> Packages { get; private set; } = new List<PackageInfo>();

            public bool Visit(IAstNode node, object parameter)
            {
                KeywordIdentifierStatement kis = node as KeywordIdentifierStatement;
                if (kis != null)
                {
                    if (kis.Keyword.Token.IsKeywordText(node.Root.TextProvider, "library") && kis.Identifier != null)
                    {
                        string packageName = node.Root.TextProvider.GetText(kis.Identifier.Token);
                        PackageInfo packageInfo = new PackageInfo(packageName);

                        packageInfo.LoadFunctionsAsync();
                        this.Packages.Add(packageInfo);
                    }
                }

                return true;
            }
        }
    }
}


//private void OnResponseDataReady(object sender, string data)
//{
//    EngineResponse response = RCompletionEngine.HelpDataSource.GetFunctionHelp("abs", "base").Result;

//    CompletionData completionData = new CompletionData()
//    {
//        Completion = completion,
//        Session = context.Session
//    };

//    response.Tag = completionData;
//    response.DataReady += OnResponseDataReady;

//    if (response.IsReady)
//    {
//        PopulateCompletionData(response);
//    }

//    EngineResponse response = sender as EngineResponse;
//    PopulateCompletionData(response);
//}

//private void PopulateCompletionData(EngineResponse response)
//{
//    if (response.Data != null)
//    {
//        CompletionData completionData = response.Tag as CompletionData;
//        if (!completionData.Session.IsDismissed)
//        {
//            RdFunctionInfo functionInfo = RdParser.GetFunctionInfo(completionData.Completion.InsertionText, response.Data);
//            completionData.Completion.Description = functionInfo != null ? functionInfo.Description : string.Empty;
//        }
//    }
//}

//private class CompletionData
//{
//    public RCompletion Completion;
//    public ICompletionSession Session;
//}
