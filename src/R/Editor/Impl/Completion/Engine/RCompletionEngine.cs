using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Completion.Providers;
using Microsoft.R.Editor.Tree.Search;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Packages;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion.Engine
{
    internal static class RCompletionEngine
    {
        private static IEnumerable<Lazy<IRCompletionListProvider>> CompletionProviders { get; set; }
        private static RHelpDataSource _helpDataSource;

        /// <summary>
        /// Provides list of completion entries for a given location in the AST.
        /// </summary>
        /// <param name="tree">Document tree</param>
        /// <param name="position">Caret position in the document</param>
        /// <param name="autoShownCompletion">True if completion is forced (like when typing Ctrl+Space)</param>
        /// <returns>List of completion entries for a given location in the AST</returns>
        public static IReadOnlyCollection<IRCompletionListProvider> GetCompletionForLocation(AstRoot ast, ITextBuffer textBuffer, int position, bool autoShownCompletion)
        {
            List<IRCompletionListProvider> providers = new List<IRCompletionListProvider>();

            if (ast.Comments.Contains(position))
            {
                // No completion in comments
                return providers;
            }

            IAstNode node = ast.NodeFromPosition(position);
            if ((node is TokenNode) && ((TokenNode)node).Token.TokenType == RTokenType.String)
            {
                // No completion in strings
                return providers;
            }

            if (IsPackageListCompletion(textBuffer, position))
            {
                providers.Add(new PackagesCompletionProvider());
            }
            else
            {
                foreach (var p in CompletionProviders)
                {
                    providers.Add(p.Value);
                }
            }

            return providers;
        }

        public static void Initialize()
        {
            if (_helpDataSource == null)
            {
                _helpDataSource = new RHelpDataSource();
            }

            if (CompletionProviders == null)
            {
                CompletionProviders = ComponentLocator<IRCompletionListProvider>.ImportMany();
            }

            EditorShell.OnIdle += OnIdle;
        }

        private static void OnIdle(object sender, EventArgs e)
        {
            EditorShell.OnIdle -= OnIdle;

            Task.Run(async () =>
            {
                IEnumerable<PackageInfo> packages = PackagesDataSource.GetBasePackages();

                // Get list of functions in the package
                foreach (PackageInfo pkg in packages)
                {
                    if (!pkg.IsLoaded)
                    {
                        await pkg.LoadFunctionsAsync();
                    }
                }
            });
        }

        public static async Task<EngineResponse> GetFunctionHelp(AstRoot ast, string functionName)
        {
            IEnumerable<IPackageInfo> packages = ast.GetPackages();

            // Get list of functions in the package
            foreach (IPackageInfo pkg in packages)
            {
                if (pkg.IsLoaded)
                {
                    if (pkg.ContainsFunction(functionName))
                    {
                        // Get collection of function signatures from documentation (parsed RD file)
                        return await _helpDataSource.GetFunctionHelp(functionName, pkg.Name);
                    }
                }
            }

            return null;
        }

        private static bool IsPackageListCompletion(ITextBuffer textBuffer, int position)
        {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            string lineText = line.GetText();
            int linePosition = position - line.Start;

            // We should be either at library(| or inside library(|) 
            // or over package name like in library(ba|se)

            // Go left and right looking for 
            RTokenizer tokenizer = new RTokenizer();
            ITextProvider textProvider = new TextStream(lineText);
            IReadOnlyTextRangeCollection<RToken> c = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            TokenStream<RToken> tokens = new TokenStream<RToken>(c, RToken.EndOfStreamToken);

            while (!tokens.IsEndOfStream())
            {
                if (tokens.CurrentToken.Start >= linePosition)
                {
                    break;
                }

                if (tokens.CurrentToken.IsKeywordText(textProvider, "library"))
                {
                    tokens.MoveToNextToken();
                    if (tokens.CurrentToken.TokenType == RTokenType.OpenBrace)
                    {
                        RToken openBrace = tokens.CurrentToken;
                        while (!tokens.IsEndOfStream())
                        {
                            if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace)
                            {
                                if (linePosition >= openBrace.End && linePosition <= tokens.CurrentToken.Start)
                                {
                                    return true;
                                }

                                return false;
                            }

                            tokens.MoveToNextToken();
                        }
                    }
                }

                tokens.MoveToNextToken();
            }

            return false;
        }
    }
}
