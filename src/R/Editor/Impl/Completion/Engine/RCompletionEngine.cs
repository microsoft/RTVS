// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Completion.Providers;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion.Engine {
    internal static class RCompletionEngine {
        private static IEnumerable<Lazy<IRCompletionListProvider>> _completionProviders;

        /// <summary>
        /// Provides list of completion entries for a given location in the AST.
        /// </summary>
        /// <param name="tree">Document tree</param>
        /// <param name="position">Caret position in the document</param>
        /// <param name="autoShownCompletion">True if completion is forced (like when typing Ctrl+Space)</param>
        /// <returns>List of completion entries for a given location in the AST</returns>
        public static IReadOnlyCollection<IRCompletionListProvider> GetCompletionForLocation(RCompletionContext context, bool autoShownCompletion) {
            List<IRCompletionListProvider> providers = new List<IRCompletionListProvider>();

            if (context.AstRoot.Comments.Contains(context.Position)) {
                // No completion in comments
                return providers;
            }

            // First check file completion - it happens inside strings
            string directory;
            if (CanShowFileCompletion(context.AstRoot, context.Position, out directory)) {
                if (!string.IsNullOrEmpty(directory)) {
                    providers.Add(new FilesCompletionProvider(directory));
                }
                return providers;
            }

            // Now check if position is inside a string, number or identifier 
            // and if so, suppress the completion list
            var tokenNode = context.AstRoot.GetNodeOfTypeFromPosition<TokenNode>(context.Position);
            if (tokenNode != null && (tokenNode.Token.TokenType == RTokenType.String ||
                                      tokenNode.Token.TokenType == RTokenType.Number ||
                                      tokenNode.Token.TokenType == RTokenType.Complex ||
                                      tokenNode.Token.TokenType == RTokenType.Identifier)) {
                // No completion in strings or numbers
                return providers;
            }

            // Check end of numeric token like 2.- dot should not be bringing completion
            tokenNode = context.AstRoot.GetNodeOfTypeFromPosition<TokenNode>(Math.Max(0, context.Position - 1));
            if (tokenNode != null && (tokenNode.Token.TokenType == RTokenType.Number ||
                                      tokenNode.Token.TokenType == RTokenType.Complex)) {
                // No completion in numbers
                return providers;
            }

            if (IsInFunctionArgumentName<FunctionDefinition>(context.AstRoot, context.Position)) {
                // No completion in function definition argument names
                return providers;
            }

            if (IsInObjectMemberName(context.AstRoot.TextProvider, context.Position)) {
                providers.Add(new WorkspaceVariableCompletionProvider());
                return providers;
            }

            if (IsPackageListCompletion(context.TextBuffer, context.Position)) {
                providers.Add(new PackagesCompletionProvider());
            } else {
                if (IsInFunctionArgumentName<FunctionCall>(context.AstRoot, context.Position)) {
                    providers.Add(new ParameterNameCompletionProvider());
                }

                foreach (var p in CompletionProviders) {
                    providers.Add(p.Value);
                }

                if (!context.IsInNameSpace()) {
                    providers.Add(new PackagesCompletionProvider());
                }
            }

            providers.Add(new WorkspaceVariableCompletionProvider());

            return providers;
        }

        public static void Initialize() {
            FunctionIndex.Initialize();
        }

        public static bool CanShowFileCompletion(AstRoot ast, int position, out string directory) {
            TokenNode node = ast.GetNodeOfTypeFromPosition<TokenNode>(position);
            directory = null;
            if ((node is TokenNode) && ((TokenNode)node).Token.TokenType == RTokenType.String) {
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

        private static IEnumerable<Lazy<IRCompletionListProvider>> CompletionProviders {
            get {
                if (_completionProviders == null) {
                    _completionProviders = ComponentLocator<IRCompletionListProvider>.ImportMany();
                }
                return _completionProviders;
            }
        }

        internal static bool IsPackageListCompletion(ITextBuffer textBuffer, int position) {
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

        /// <summary>
        /// Determines if position is in the argument name. Typically used to
        ///     a) suppress general intellisense when typing function arguments 
        ///         in a function/ definition such as in 'x &lt;- function(a|'
        ///     b) determine if completion list should contain argumet names
        ///        when user types inside function call.
        /// </summary>
        internal static bool IsInFunctionArgumentName<T>(AstRoot ast, int position) where T : class, IFunction {
            T funcDef = ast.GetNodeOfTypeFromPosition<T>(position);
            if (funcDef == null || funcDef.OpenBrace == null || funcDef.Arguments == null) {
                return false;
            }

            if (position < funcDef.OpenBrace.End || position >= funcDef.SignatureEnd) {
                return false;
            }

            int start = funcDef.OpenBrace.End;
            int end = funcDef.SignatureEnd;

            if (funcDef.Arguments.Count == 0 && position >= start && position <= end) {
                return true;
            }

            for (int i = 0; i < funcDef.Arguments.Count; i++) {
                CommaSeparatedItem csi = funcDef.Arguments[i];
                NamedArgument na = csi as NamedArgument;

                if (position < csi.Start) {
                    break;
                }

                end = csi.End;
                if (position >= start && position <= end) {
                    if (na == null) {
                        return true;
                    }

                    if (position <= na.EqualsSign.Start) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if position is in object member. Typically used
        /// to suppress general intellisense when typing data member 
        /// name such as 'mtcars$|'
        /// </summary>
        internal static bool IsInObjectMemberName(ITextProvider textProvider, int position) {
            if (position > 0) {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = textProvider[i];

                    if (ch == '$' || ch == '@') {
                        return true;
                    }

                    if (!RTokenizer.IsIdentifierCharacter(ch)) {
                        break;
                    }
                }
            }

            return false;
        }
    }
}
