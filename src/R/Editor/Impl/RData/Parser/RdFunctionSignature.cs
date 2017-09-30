// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.RData.Tokens;

namespace Microsoft.R.Editor.RData.Parser {
    internal static class RdFunctionSignature {
        public static IReadOnlyList<ISignatureInfo> ExtractSignatures(RdParseContext context) {
            // \usage{
            //    loglm1(formula, data, \dots)
            //    \method{loglm1}{xtabs}(formula, data, \dots)
            //    \method{loglm1}{data.frame}(formula, data, \dots)
            //    \method{loglm1}{default}(formula, data, start = rep(1, length(data)), fitted = FALSE,
            //                             keep.frequencies = fitted, param = TRUE, eps = 1 / 10,
            //                             iter = 40, print = FALSE, \dots)
            // }
            //
            // Signatures can be for multiple related functions
            //  }\usage{
            //      lockEnvironment(env, bindings = FALSE)
            //      environmentIsLocked(env)
            //      lockBinding(sym, env)
            //      unlockBinding(sym, env)
            //      bindingIsLocked(sym, env)

            var tokens = context.Tokens;
            var signatures = new List<ISignatureInfo>();

            // Must be at '\usage{'
            if (RdParseUtility.GetKeywordArgumentBounds(tokens, out var startTokenIndex, out var endTokenIndex)) {
                // Get inner content of the \usage{...} block cleaned up for R parsing
                var usage = GetRText(context, startTokenIndex, endTokenIndex);
                var sigs = ParseSignatures(usage);
                if (sigs != null) {
                    signatures.AddRange(sigs);
                }
                tokens.Position = endTokenIndex;
            }

            return signatures;
        }

        /// <summary>
        /// Extracts R-parseable text from RD \usage{...} block.
        /// RD text may contain \dots sequence  which denotes ellipsis.
        /// R parser does not know  about it and hence we must replace \dots by ...
        /// Also, signatures may contain S3 method info like 
        /// '\method{as.matrix}{data.frame}(x, rownames.force = NA, \dots)'
        /// which we need to filter out since they are irrelevant to intellisense.
        /// </summary>
        private static string GetRText(RdParseContext context, int startTokenIndex, int endTokenIndex) {
            var sb = new StringBuilder();
            for (var i = startTokenIndex; i < endTokenIndex; i++) {
                int fragmentStart;
                int fragmentEnd;

                var token = context.Tokens[i];
                if (token.TokenType == RdTokenType.Keyword && context.TextProvider.GetText(token) == "\\method") {
                    fragmentStart = SkipS3Method(context, ref i);
                    fragmentEnd = context.Tokens[i].Start;
                } else {
                    if (token.TokenType == RdTokenType.Keyword && context.TextProvider.GetText(token) == "\\dots") {
                        sb.Append("...");
                    } else if (token.TokenType == RdTokenType.OpenSquareBracket || token.TokenType == RdTokenType.CloseSquareBracket) {
                        // Copy verbatim
                        sb.Append(context.TextProvider.GetText(token));
                    }
                    fragmentStart = context.Tokens[i].End;
                    fragmentEnd = context.Tokens[i + 1].Start;
                }

                Debug.Assert(fragmentStart <= fragmentEnd);
                if (fragmentStart <= fragmentEnd) {
                    var range = TextRange.FromBounds(fragmentStart, fragmentEnd);
                    var fragment = context.TextProvider.GetText(range);
                    sb.Append(fragment);
                } else {
                    break; // Something went wrong;
                }
            }
            return sb.ToString().Trim();
        }

        private static int SkipS3Method(RdParseContext context, ref int index) {
            var token = context.Tokens[index];
            Debug.Assert(token.TokenType == RdTokenType.Keyword && context.TextProvider.GetText(token) == "\\method");

            index++;
            for (var i = 0; i < 2; i++) {
                if (context.Tokens[index].TokenType == RdTokenType.OpenCurlyBrace) {
                    index++;
                }
                if (context.Tokens[index].TokenType == RdTokenType.CloseCurlyBrace) {
                    index++;
                }
            }
            // Should be past \method{...}{...}. Now skip signature
            var bc = new BraceCounter<char>(new[] { '(', ')' });
            for (var i = context.Tokens[index - 1].End; i < context.TextProvider.Length; i++) {
                if (bc.CountBrace(context.TextProvider[i])) {
                    if (bc.Count == 0) {
                        // Calculate index of the next token after text position 'i'
                        index = context.Tokens.Length - 1;
                        for (var j = index; j < context.Tokens.Length; j++) {
                            if (context.Tokens[j].Start >= i) {
                                index = j;
                                break;
                            }
                        }
                        return i + 1;
                    }
                }
            }
            return context.Tokens[index].End;
        }

        public static IReadOnlyList<ISignatureInfo> ParseSignatures(string usageContent) {
            // RD signature text may contain \dots sequence  which denotes ellipsis.
            // R parser does not know  about it and hence we will replace \dots by ...
            // Also, signatures may contain S3 method info like 
            // '\method{as.matrix}{data.frame}(x, rownames.force = NA, \dots)'
            // which we need to filter out since they are irrelevant to intellisense.

            var signatures = new List<ISignatureInfo>();
            usageContent = usageContent.Replace(@"\dots", "...");

            var tokenizer = new RTokenizer(separateComments: true);
            var collection = tokenizer.Tokenize(usageContent);
            var textProvider = new TextStream(usageContent);
            var tokens = new TokenStream<RToken>(collection, RToken.EndOfStreamToken);

            var parseContext = new ParseContext(textProvider,
                         TextRange.FromBounds(tokens.CurrentToken.Start, textProvider.Length),
                         tokens, tokenizer.CommentTokens);

            while (!tokens.IsEndOfStream()) {
                if (tokens.CurrentToken.TokenType != RTokenType.Identifier) {
                    break;
                }

                var functionName = textProvider.GetText(tokens.CurrentToken);
                tokens.MoveToNextToken();

                var info = ParseSignature(functionName, parseContext);
                if (info != null) {
                    signatures.Add(info);
                }
            }

            return signatures;
        }

        private static ISignatureInfo ParseSignature(string functionName, ParseContext context) {
            var info = new SignatureInfo(functionName);
            var signatureArguments = new List<IArgumentInfo>();

            // RD data may contain function name(s) without braces
            if (context.Tokens.CurrentToken.TokenType == RTokenType.OpenBrace) {
                var functionCall = new FunctionCall();
                functionCall.Parse(context, context.AstRoot);

                foreach (var arg in functionCall.Arguments) {
                    string argName = null;
                    string argDefaultValue = null;
                    var isEllipsis = false;

                    if (arg is ExpressionArgument expArg) {
                        argName = context.TextProvider.GetText(expArg.ArgumentValue);
                    } else {
                        if (arg is NamedArgument nameArg) {
                            argName = context.TextProvider.GetText(nameArg.NameRange);
                            argDefaultValue = nameArg.DefaultValue != null ?
                                RdText.CleanRawRdText(context.TextProvider.GetText(nameArg.DefaultValue)) : string.Empty;
                        } else if (arg is MissingArgument) {
                            argName = string.Empty;
                        } else if (arg is EllipsisArgument) {
                            argName = "...";
                            isEllipsis = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(argName)) {
                        var argInfo = new ArgumentInfo(argName) {
                            DefaultValue = argDefaultValue,
                            IsEllipsis = isEllipsis,
                            IsOptional = false
                        };
                        // TODO: actually parse
                        signatureArguments.Add(argInfo);
                    }
                }
            }

            info.Arguments = signatureArguments;
            return info;
        }
    }
}
