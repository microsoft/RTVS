// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completions.Providers;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Completions.Engine {
    public sealed class RCompletionEngine : IRCompletionEngine {
        private readonly IServiceContainer _services;
        private readonly IImageService _imageService;

        public RCompletionEngine(IServiceContainer services) {
            _services = services;
            _imageService = services.GetService<IImageService>();
        }

        /// <summary>
        /// Provides list of completion entries for a given location in the AST.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>List of completion entries for a given location in the AST</returns>
        public IReadOnlyCollection<IRCompletionListProvider> GetCompletionForLocation(IRIntellisenseContext context) {
            var ast = context.AstRoot;

            var providers = new List<IRCompletionListProvider>();
            if (ast.Comments.Contains(context.Position)) {
                // No completion in comments except iif it is Roxygen
                providers.Add(new RoxygenTagCompletionProvider(_imageService));
                return providers;
            }

            if (context.IsRHistoryRequest) {
                var history = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().History;
                providers.Add(new RHistoryCompletionProvider(history, _imageService));
                return providers;
            }

            // First check file completion - it happens inside strings
            if (CanShowFileCompletion(ast, context.Position, out var directory)) {
                if (!string.IsNullOrEmpty(directory)) {
                    providers.Add(new FilesCompletionProvider(directory, _services));
                }
                return providers;
            }

            // Special case for unterminated strings
            var tokenNode = ast.GetNodeOfTypeFromPosition<TokenNode>(context.Position, includeEnd: true);
            if (tokenNode != null && context.Position == tokenNode.End && tokenNode.Token.TokenType == RTokenType.String) {
                var snapshot = context.EditorBuffer.CurrentSnapshot;
                // String token at least has opening quote
                char quote = snapshot[tokenNode.Start];
                if (tokenNode.Length == 1 || quote != snapshot[tokenNode.End - 1]) {
                    // No completion at the end of underminated string
                    return providers;
                }
            }

            // Now check if position is inside a string or a number and if so, suppress the completion list
            tokenNode = ast.GetNodeOfTypeFromPosition<TokenNode>(context.Position);
            if (tokenNode != null && (tokenNode.Token.TokenType == RTokenType.String ||
                                      tokenNode.Token.TokenType == RTokenType.Number ||
                                      tokenNode.Token.TokenType == RTokenType.Complex)) {
                // No completion in strings or numbers
                return providers;
            }

            // We do not want automatic completion inside identifiers such as in a middle
            // of ab|c or in `abc|`. Manually invoked completion is fine.
            if (tokenNode != null && tokenNode.Token.TokenType == RTokenType.Identifier && context.AutoShownCompletion) {
                return providers;
            }

            // Check end of numeric token like 2.- dot should not be bringing completion
            tokenNode = ast.GetNodeOfTypeFromPosition<TokenNode>(Math.Max(0, context.Position - 1));
            if (tokenNode != null && (tokenNode.Token.TokenType == RTokenType.Number ||
                                      tokenNode.Token.TokenType == RTokenType.Complex)) {
                // No completion in numbers
                return providers;
            }

            if (ast.IsInFunctionArgumentName<FunctionDefinition>(context.Position)) {
                // No completion in function definition argument names
                return providers;
            }

            var variablesProvider = _services.GetService<IVariablesProvider>();
            var packageIndex = _services.GetService<IPackageIndex>();

            if (ast.TextProvider.IsInObjectMemberName(context.Position)) {
                 providers.Add(new WorkspaceVariableCompletionProvider(variablesProvider, _imageService));
                return providers;
            }

            if (IsPackageListCompletion(context.EditorBuffer, context.Position)) {
                providers.Add(new PackagesCompletionProvider(packageIndex, _imageService));
            } else {
                var functionIndex = _services.GetService<IFunctionIndex>();
                providers.Add(new ParameterNameCompletionProvider(functionIndex, _imageService));
                providers.Add(new KeywordCompletionProvider(_services));
                providers.Add(new PackageFunctionCompletionProvider(_services));
                providers.Add(new UserVariablesCompletionProvider(_imageService));
                providers.Add(new SnippetCompletionProvider(_services));

                if (!context.IsCaretInNamespace()) {
                    providers.Add(new PackagesCompletionProvider(packageIndex, _imageService));
                }
            }

            if (!context.IsCaretInNamespace()) {
                providers.Add(new WorkspaceVariableCompletionProvider(variablesProvider, _imageService));
            }

            return providers;
        }

        public static bool CanShowFileCompletion(AstRoot ast, int position, out string directory) {
            TokenNode node = ast.GetNodeOfTypeFromPosition<TokenNode>(position);
            directory = null;
            if (node != null && node.Token.TokenType == RTokenType.String) {
                string text = node.Root.TextProvider.GetText(node);
                // Bring file/folder completion when either string is empty or ends with /
                // assuming that / specifies directory where files are.
                if (text.Length == 2 || text.EndsWith("/\"", StringComparison.Ordinal) || text.EndsWith("/\'", StringComparison.Ordinal)) {
                    directory = text;
                    return true;
                }
            }
            return false;
        }

        internal static bool IsPackageListCompletion(IEditorBuffer editorBuffer, int position) {
            var snapshot = editorBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(position);
            string lineText = line.GetText();
            int linePosition = position - line.Start;

            // We should be either at library(| or inside library(|) 
            // or over package name like in library(ba|se)

            // Go left and right looking for 
            RTokenizer tokenizer = new RTokenizer();
            ITextProvider textProvider = new TextStream(lineText);
            IReadOnlyTextRangeCollection<RToken> c = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            TokenStream<RToken> tokens = new TokenStream<RToken>(c, RToken.EndOfStreamToken);

            while (!tokens.IsEndOfStream()) {
                if (tokens.CurrentToken.Start >= linePosition) {
                    break;
                }

                if (tokens.CurrentToken.TokenType == RTokenType.Identifier) {
                    string identifier = textProvider.GetText(tokens.CurrentToken);
                    if (identifier == "library" || identifier == "require") {
                        tokens.MoveToNextToken();

                        if (tokens.CurrentToken.TokenType == RTokenType.OpenBrace) {
                            RToken openBrace = tokens.CurrentToken;
                            while (!tokens.IsEndOfStream()) {
                                if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace) {
                                    if (linePosition >= openBrace.End && linePosition <= tokens.CurrentToken.Start) {
                                        return true;
                                    }
                                    return false;
                                } else if (tokens.NextToken.TokenType == RTokenType.EndOfStream) {
                                    return true;
                                }
                                tokens.MoveToNextToken();
                            }
                        }
                    }
                }
                tokens.MoveToNextToken();
            }
            return false;
        }
    }
}
