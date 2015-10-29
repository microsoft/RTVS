using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser {
    static class RdFunctionSignature {
        public static IReadOnlyList<ISignatureInfo> ExtractSignatures(RdParseContext context) {
            // \usage{
            //    loglm1(formula, data, \dots)
            //    \method{loglm1}{xtabs}(formula, data, \dots)
            //    \method{loglm1}{data.frame}(formula, data, \dots)
            //    \method{loglm1}{default}(formula, data, start = rep(1, length(data)), fitted = FALSE,
            //                             keep.frequencies = fitted, param = TRUE, eps = 1 / 10,
            //                             iter = 40, print = FALSE, \dots)
            // }

            TokenStream<RdToken> tokens = context.Tokens;
            List<ISignatureInfo> signatures = new List<ISignatureInfo>();

            int startTokenIndex, endTokenIndex;
            if (RdParseUtility.GetKeywordArgumentBounds(tokens, out startTokenIndex, out endTokenIndex)) {
                // Counting ( and ) find the end of the signature string
                int usageBlockStart = tokens[startTokenIndex].End;
                int usageBlockEnd = FindEndOfSignatureText(context, usageBlockStart);
                if (usageBlockEnd > usageBlockStart) {
                    TextRange range = TextRange.FromBounds(usageBlockStart, usageBlockEnd);
                    string usage = context.TextProvider.GetText(range).Trim();

                    ISignatureInfo sig = ParseSignature(usage);
                    if (sig != null) {
                        signatures.Add(sig);
                    }

                    while (!tokens.IsEndOfStream() && tokens.Position < usageBlockEnd) {
                        RdToken token = tokens.CurrentToken;

                        if (context.IsAtKeywordWithParameters(@"\method")) {
                            sig = ParseMethod(context);
                            if (sig == null) {
                                break;
                            }

                            signatures.Add(sig);
                        } else {
                            tokens.MoveToNextToken();
                        }
                    }
                }

                tokens.Position = endTokenIndex;
            }

            return signatures;
        }

        /// <summary>
        /// Parses \method{name}{suffix}(formula, data, \dots)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static ISignatureInfo ParseMethod(RdParseContext context) {
            TokenStream<RdToken> tokens = context.Tokens;

            // Move past \method{
            tokens.Advance(2);

            // Expected '}{suffix}'
            if (tokens.CurrentToken.TokenType == RdTokenType.CloseCurlyBrace &&
                tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace &&
                tokens.LookAhead(2).TokenType == RdTokenType.CloseCurlyBrace) {
                // Move past '{suffix'
                tokens.Advance(2);

                TextRange range = TextRange.FromBounds(tokens.PreviousToken.End, tokens.CurrentToken.Start);
                string suffix = context.TextProvider.GetText(range).Trim();

                // Skip final '}'
                tokens.MoveToNextToken();

                // Should be at '(formula, data, \dots)'
                int start = tokens.PreviousToken.End;
                int end = FindEndOfSignatureText(context, start);
                if (end > start) {
                    // Advance token stream to be past the end of the signature
                    while (!tokens.IsEndOfStream() && tokens.CurrentToken.Start < end) {
                        tokens.MoveToNextToken();
                    }

                    range = TextRange.FromBounds(start, end);
                    string signatureText = context.TextProvider.GetText(range).Trim();

                    return ParseSignature(signatureText);
                }
            }

            return null;
        }

        private static int FindEndOfSignatureText(RdParseContext context, int start) {
            // Counting ( and ) find the end of the signature.
            BraceCounter<char> braceCounter = new BraceCounter<char>('(', ')');
            for (int i = start; i < context.TextProvider.Length; i++) {
                if (braceCounter.CountBrace(context.TextProvider[i])) {
                    if (braceCounter.Count == 0)
                        return i + 1;
                }
            }

            return -1;
        }

        public static SignatureInfo ParseSignature(string text, IReadOnlyDictionary<string, string> argumentsDescriptions = null) {
            // RD signature text may contain \dots sequence 
            // which denotes ellipsis. R parser does not know 
            // about it and hence we will replace \dots by ...

            text = text.Replace(@"\dots", "...");

            int openBraceIndex = text.IndexOf('(');
            if (openBraceIndex < 0) {
                return null;
            }

            text = text.Substring(openBraceIndex);

            RTokenizer tokenizer = new RTokenizer(separateComments: true);
            IReadOnlyTextRangeCollection<RToken> collection = tokenizer.Tokenize(text);
            ITextProvider textProvider = new TextStream(text);
            TokenStream<RToken> tokens = new TokenStream<RToken>(collection, RToken.EndOfStreamToken);

            SignatureInfo info = new SignatureInfo();
            List<IArgumentInfo> signatureArguments = new List<IArgumentInfo>();

            var rParseContext = new Microsoft.R.Core.Parser.ParseContext(textProvider, new TextRange(0, text.Length), tokens, tokenizer.CommentTokens);

            FunctionCall functionCall = new FunctionCall();
            functionCall.Parse(rParseContext, rParseContext.AstRoot);

            for (int i = 0; i < functionCall.Arguments.Count; i++) {
                IAstNode arg = functionCall.Arguments[i];

                string argName = null;
                string argDefaultValue = null;
                bool isEllipsis = false;
                bool isOptional = false;

                ExpressionArgument expArg = arg as ExpressionArgument;
                if (expArg != null) {
                    argName = textProvider.GetText(expArg.ArgumentValue);
                } else {
                    NamedArgument nameArg = arg as NamedArgument;
                    if (nameArg != null) {
                        argName = textProvider.GetText(nameArg.NameRange);
                        argDefaultValue = RdText.CleanRawRdText(textProvider.GetText(nameArg.DefaultValue));
                    } else {
                        MissingArgument missingArg = arg as MissingArgument;
                        if (missingArg != null) {
                            argName = string.Empty;
                        } else {
                            EllipsisArgument ellipsisArg = arg as EllipsisArgument;
                            if (ellipsisArg != null) {
                                argName = "...";
                                isEllipsis = true;
                            }
                        }
                    }
                }

                ArgumentInfo argInfo = new ArgumentInfo(argName);
                argInfo.DefaultValue = argDefaultValue;
                argInfo.IsEllipsis = isEllipsis;
                argInfo.IsOptional = isOptional; // TODO: actually parse

                if (argumentsDescriptions != null) {
                    string description;
                    if (argumentsDescriptions.TryGetValue(argName, out description)) {
                        argInfo.Description = description;
                    }
                }

                signatureArguments.Add(argInfo);
            }

            info.Arguments = signatureArguments;
            return info;
        }
    }
}
