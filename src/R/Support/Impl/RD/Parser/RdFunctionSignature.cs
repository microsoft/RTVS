// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Support.Help;
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
            //
            // Signatures can be for multiple related functions
            //  }\usage{
            //      lockEnvironment(env, bindings = FALSE)
            //      environmentIsLocked(env)
            //      lockBinding(sym, env)
            //      unlockBinding(sym, env)
            //      bindingIsLocked(sym, env)

            TokenStream<RdToken> tokens = context.Tokens;
            List<ISignatureInfo> signatures = new List<ISignatureInfo>();

            // Must be at '\usage{'
            int startTokenIndex, endTokenIndex;
            if (RdParseUtility.GetKeywordArgumentBounds(tokens, out startTokenIndex, out endTokenIndex)) {
                // Get inner content of the \usage{...} block cleaned up for R parsing
                string usage = GetRText(context, startTokenIndex, endTokenIndex);
                IReadOnlyList<ISignatureInfo> sigs = ParseSignatures(usage);
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
            StringBuilder sb = new StringBuilder();
            for (int i = startTokenIndex; i < endTokenIndex; i++) {
                int fragmentStart;
                int fragmentEnd;

                RdToken token = context.Tokens[i];
                if (token.TokenType == RdTokenType.Keyword && context.TextProvider.GetText(token) == "\\method") {
                    fragmentStart = SkipS3Method(context, ref i);
                    fragmentEnd = context.Tokens[i].Start;
                } else {
                    if (token.TokenType == RdTokenType.Keyword && context.TextProvider.GetText(token) == "\\dots") {
                        sb.Append("...");
                    }
                    fragmentStart = context.Tokens[i].End;
                    fragmentEnd = context.Tokens[i + 1].Start;
                }

                Debug.Assert(fragmentStart <= fragmentEnd);
                if (fragmentStart <= fragmentEnd) {
                    ITextRange range = TextRange.FromBounds(fragmentStart, fragmentEnd);
                    string fragment = context.TextProvider.GetText(range);
                    sb.Append(fragment);
                }
                else {
                    break; // Something went wrong;
                }
            }
            return sb.ToString().Trim();
        }

        private static int SkipS3Method(RdParseContext context, ref int index) {
            RdToken token = context.Tokens[index];
            Debug.Assert(token.TokenType == RdTokenType.Keyword && context.TextProvider.GetText(token) == "\\method");

            index++;
            for (int i = 0; i < 2; i++) {
                if (context.Tokens[index].TokenType == RdTokenType.OpenCurlyBrace) {
                    index++;
                }
                if (context.Tokens[index].TokenType == RdTokenType.CloseCurlyBrace) {
                    index++;
                }
            }
            // Should be past \method{...}{...}. Now skip signature
            BraceCounter<char> bc = new BraceCounter<char>(new char[] { '(', ')' });
            for (int i = context.Tokens[index - 1].End; i < context.TextProvider.Length; i++) {
                if (bc.CountBrace(context.TextProvider[i])) {
                    if (bc.Count == 0) {
                        // Calculate index of the next token after text position 'i'
                        index = context.Tokens.Length - 1;
                        for (int j = index; j < context.Tokens.Length; j++) {
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

            List<ISignatureInfo> signatures = new List<ISignatureInfo>();
            usageContent = usageContent.Replace(@"\dots", "...");

            RTokenizer tokenizer = new RTokenizer(separateComments: true);
            IReadOnlyTextRangeCollection<RToken> collection = tokenizer.Tokenize(usageContent);
            ITextProvider textProvider = new TextStream(usageContent);
            TokenStream<RToken> tokens = new TokenStream<RToken>(collection, RToken.EndOfStreamToken);

            var parseContext = new ParseContext(textProvider,
                         TextRange.FromBounds(tokens.CurrentToken.Start, textProvider.Length),
                         tokens, tokenizer.CommentTokens);

            while (!tokens.IsEndOfStream()) {
                // Filter out '\method{...}{}(signature)
                if (tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace) {
                    // Check if { is preceded by \method
                }

                if (tokens.CurrentToken.TokenType != RTokenType.Identifier) {
                    break;
                }

                string functionName = textProvider.GetText(tokens.CurrentToken);
                tokens.MoveToNextToken();

                ISignatureInfo info = ParseSignature(functionName, parseContext);
                if (info != null) {
                    signatures.Add(info);
                }
            }

            return signatures;
        }

        private static ISignatureInfo ParseSignature(string functionName, ParseContext context) {
            SignatureInfo info = new SignatureInfo(functionName);
            List<IArgumentInfo> signatureArguments = new List<IArgumentInfo>();

            // RD data may contain function name(s) without braces
            if (context.Tokens.CurrentToken.TokenType == RTokenType.OpenBrace) {
                FunctionCall functionCall = new FunctionCall();
                functionCall.Parse(context, context.AstRoot);

                for (int i = 0; i < functionCall.Arguments.Count; i++) {
                    IAstNode arg = functionCall.Arguments[i];

                    string argName = null;
                    string argDefaultValue = null;
                    bool isEllipsis = false;
                    bool isOptional = false;

                    ExpressionArgument expArg = arg as ExpressionArgument;
                    if (expArg != null) {
                        argName = context.TextProvider.GetText(expArg.ArgumentValue);
                    } else {
                        NamedArgument nameArg = arg as NamedArgument;
                        if (nameArg != null) {
                            argName = context.TextProvider.GetText(nameArg.NameRange);
                            argDefaultValue = nameArg.DefaultValue != null ? 
                                 RdText.CleanRawRdText(context.TextProvider.GetText(nameArg.DefaultValue)) : string.Empty;
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

                    signatureArguments.Add(argInfo);
                }
            }

            info.Arguments = signatureArguments;
            return info;
        }
    }
}
