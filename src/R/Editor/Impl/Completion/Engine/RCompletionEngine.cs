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
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
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
            IREditorDocument document = REditorDocument.FindInProjectedBuffers(context.Session.TextView.TextBuffer);

            if (context.AstRoot.Comments.Contains(context.Position)) {
                // No completion in comments
                return providers;
            }

            IAstNode node = context.AstRoot.NodeFromPosition(context.Position);
            if ((node is TokenNode) && ((TokenNode)node).Token.TokenType == RTokenType.String) {
                // No completion in strings
                return providers;
            }

            if (IsInFunctionDefinitionArgumentName(context.AstRoot, context.Position)) {
                // No completion in function definition argument names
                return providers;
            }

            if (IsInObjectMemberName(context.AstRoot.TextProvider, context.Position)) {
                providers.Add(new WorkspaceVaraibleCompletionProvider());
                return providers;
            }

            if (IsPackageListCompletion(context.TextBuffer, context.Position)) {
                providers.Add(new PackagesCompletionProvider());
            } else {
                foreach (var p in CompletionProviders) {
                    providers.Add(p.Value);
                }

                if (!context.IsInNameSpace()) {
                    providers.Add(new PackagesCompletionProvider());
                }
            }

            if (document != null && document.IsTransient) {
                providers.Add(new WorkspaceVaraibleCompletionProvider());
            }

            return providers;
        }

        public static void Initialize() {
            FunctionIndex.Initialize();
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
        /// Determines if position is in the argument name. Typically used
        /// to suppress general intellisense when typing function arguments 
        /// in a function/ definition such as in 'x &lt;- function(a|'
        /// </summary>
        internal static bool IsInFunctionDefinitionArgumentName(AstRoot ast, int position) {
            FunctionDefinition funcDef = ast.GetNodeOfTypeFromPosition<FunctionDefinition>(position);
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
                        return true; // Suppress intellisense
                    }

                    if (position <= na.EqualsSign.Start) {
                        return true; // Suppress intellisense
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
