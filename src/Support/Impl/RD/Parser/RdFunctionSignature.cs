using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    static class RdFunctionSignature
    {
        public static IReadOnlyList<ISignatureInfo> ExtractSignatures(RdParseContext context)
        {
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

            int start = tokens.Position;
            int end = RdParseUtility.FindRdKeywordArgumentBounds(tokens);

            // Skip '\usage' and '{'
            tokens.Advance(2);

            // Counting ( and ) find the end of the signature string
            int signatureStart = tokens.PreviousToken.End;
            int signatureEnd = FindEndOfSignature(context, start);

            TextRange range = TextRange.FromBounds(signatureStart, signatureEnd);
            string usage = context.TextProvider.GetText(range).Trim();

            ISignatureInfo sig = ParseSignature(usage);
            if (sig != null)
            {
                signatures.Add(sig);
            }

            while (!tokens.IsEndOfStream() && tokens.Position < end)
            {
                RdToken token = tokens.CurrentToken;

                if (tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace && token.IsKeywordText(context.TextProvider, @"\method"))
                {
                    sig = ParseMethod(context);
                    if (sig == null)
                    {
                        tokens.Position = end;
                        break;
                    }

                    signatures.Add(sig);
                }
                else
                {
                    tokens.MoveToNextToken();
                }
            }

            return signatures;
        }

        private static ISignatureInfo ParseMethod(RdParseContext context)
        {
            TokenStream<RdToken> tokens = context.Tokens;

            // \method{name}{suffix}(formula, data, \dots)
            tokens.Advance(2);

            if (tokens.CurrentToken.TokenType == RdTokenType.CloseCurlyBrace &&
                tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace &&
                tokens.LookAhead(2).TokenType == RdTokenType.CloseCurlyBrace)
            {
                tokens.Advance(3);

                TextRange range = TextRange.FromBounds(tokens.PreviousToken.End, tokens.CurrentToken.Start);
                string suffix = context.TextProvider.GetText(range).Trim();

                tokens.MoveToNextToken(); // skip last }

                int start = tokens.PreviousToken.End;
                int end = FindEndOfSignature(context, start);
                if (end >= 0)
                {
                    // Advance token stream to be past the end of the signature
                    while (!tokens.IsEndOfStream() && tokens.CurrentToken.Start < end)
                    {
                        tokens.MoveToNextToken();
                    }

                    range = TextRange.FromBounds(start, end);
                    string signatureText = context.TextProvider.GetText(range).Trim();

                    return ParseSignature(signatureText);
                }
            }

            return null;
        }

        private static int FindEndOfSignature(RdParseContext context, int start)
        {
            // Counting ( and ) find the end of the signature.
            RdBraceCounter<char> braceCounter = new RdBraceCounter<char>('(', ')');
            for (int i = start; i < context.TextProvider.Length; i++)
            {
                if (braceCounter.CountBrace(context.TextProvider[i]))
                {
                    if (braceCounter.Count == 0)
                        return i + 1;
                }
            }

            return -1;
        }

        public static SignatureInfo ParseSignature(string text, IReadOnlyDictionary<string, string> argumentsDescriptions = null)
        {
            // RD signature text may contain \dots sequence 
            // which denotes ellipsis. R parser does not know 
            // about it and hence we will replace \dots by ...

            text = text.Replace(@"\dots", "...");

            int openBraceIndex = text.IndexOf('(');
            if (openBraceIndex < 0)
            {
                return null;
            }

            text = text.Substring(openBraceIndex);

            RTokenizer tokenizer = new RTokenizer();
            IReadOnlyTextRangeCollection<RToken> collection = tokenizer.Tokenize(text);
            ITextProvider textProvider = new TextStream(text);
            TokenStream<RToken> tokens = new TokenStream<RToken>(collection, RToken.EndOfStreamToken);

            SignatureInfo info = new SignatureInfo();
            List<IArgumentInfo> signatureArguments = new List<IArgumentInfo>();

            var rParseContext = new Microsoft.R.Core.Parser.ParseContext(textProvider, new TextRange(0, text.Length), tokens);

            FunctionCall functionCall = new FunctionCall();
            functionCall.Parse(rParseContext, rParseContext.AstRoot);

            foreach (var arg in functionCall.Arguments)
            {
                string argName = null;
                string argDefaultValue = null;
                bool isEllipsis = false;
                bool isOptional = false;

                ExpressionArgument expArg = arg as ExpressionArgument;
                if (expArg != null)
                {
                    argName = textProvider.GetText(expArg);
                }
                else
                {
                    NamedArgument nameArg = arg as NamedArgument;
                    if (nameArg != null)
                    {
                        argName = textProvider.GetText(nameArg.NameRange);
                        argDefaultValue = textProvider.GetText(nameArg.DefaultValue);
                    }
                    else
                    {
                        EllipsisArgument ellipsisArg = arg as EllipsisArgument;
                        if (ellipsisArg != null)
                        {
                            argName = "...";
                            isEllipsis = true;
                        }
                    }
                }

                ArgumentInfo argInfo = new ArgumentInfo(argName);
                argInfo.DefaultValue = argDefaultValue;
                argInfo.IsEllipsis = isEllipsis;
                argInfo.IsOptional = isOptional; // TODO: actually parse

                if (argumentsDescriptions != null)
                {
                    string description;
                    if (argumentsDescriptions.TryGetValue(argName, out description))
                    {
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
