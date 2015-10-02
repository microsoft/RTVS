using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Completion.Providers;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion.Engine
{
    internal static class RCompletionEngine
    {
        private static IEnumerable<Lazy<IRCompletionListProvider>> _completionProviders;

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

            if(IsInFunctionDefinitionArgumentName(ast, position))
            {
                // No completion in function definition argument names
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
            FunctionIndex.Initialize();
        }

        private static IEnumerable<Lazy<IRCompletionListProvider>> CompletionProviders
        {
            get
            {
                if (_completionProviders == null)
                {
                    _completionProviders = ComponentLocator<IRCompletionListProvider>.ImportMany();
                }

                return _completionProviders;
            }
        }

        internal static bool IsPackageListCompletion(ITextBuffer textBuffer, int position)
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

                if (tokens.CurrentToken.IsKeywordText(textProvider, "library") ||
                    tokens.CurrentToken.IsKeywordText(textProvider, "require"))
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
                            else if (tokens.NextToken.TokenType == RTokenType.EndOfStream)
                            {
                                return true;
                            }

                            tokens.MoveToNextToken();
                        }
                    }
                }

                tokens.MoveToNextToken();
            }

            return false;
        }

        internal static bool IsInFunctionDefinitionArgumentName(AstRoot ast, int position)
        {
            FunctionDefinition funcDef = ast.GetNodeOfTypeFromPosition<FunctionDefinition>(position);
            if(funcDef == null || funcDef.OpenBrace == null || funcDef.Arguments == null)
            {
                return false;
            }

            if(position < funcDef.OpenBrace.End || position >= funcDef.SignatureEnd)
            {
                return false;
            }

            int start = funcDef.OpenBrace.End;
            int end = funcDef.SignatureEnd;

            if (funcDef.Arguments.Count == 0 && position >= start && position <= end)
            {
                return true;
            }

            for (int i = 0; i < funcDef.Arguments.Count; i++)
            {
                CommaSeparatedItem csi = funcDef.Arguments[i];
                NamedArgument na = csi as NamedArgument;

                if(position < csi.Start)
                {
                    break;
                }

                end = csi.End;
                if (position >= start && position <= end)
                {
                    if(na == null)
                    {
                        return true; // Suppress intellisense
                    }

                    if(position <= na.EqualsSign.Start)
                    {
                        return true; // Suppress intellisense
                    }
                }
            }

            return false;
        }
    }
}
