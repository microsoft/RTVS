using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    static class FunctionSignature
    {
        public static IReadOnlyCollection<SignatureInfo> ExtractSignatures(ParseContext context)
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
            List<SignatureInfo> signatures = new List<SignatureInfo>();

            int start = tokens.Position;
            int end = RdParseUtility.FindRdKeywordArgumentBounds(tokens);

            tokens.Advance(2);

            if (tokens.CurrentToken.TokenType == RdTokenType.Argument)
            {
                string usage = context.TextProvider.GetText(tokens.CurrentToken);
                tokens.MoveToNextToken();

                SignatureInfo sig = ParseSignature(usage);
                if (sig != null)
                {
                    signatures.Add(sig);
                }
            }

            while (!tokens.IsEndOfStream() && tokens.Position < end)
            {
                RdToken token = tokens.CurrentToken;

                if (tokens.NextToken.TokenType == RdTokenType.OpenBrace && token.IsKeywordText(context.TextProvider, @"\method"))
                {
                    SignatureInfo sig = ParseMethod(context);
                    if (sig == null)
                    {
                        tokens.Position = end;
                        return null;
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

        private static SignatureInfo ParseMethod(ParseContext context)
        {
            TokenStream<RdToken> tokens = context.Tokens;

            // \method{name}{suffix}(formula, data, \dots)
            tokens.Advance(2);

            if (tokens.CurrentToken.TokenType == RdTokenType.Argument &&
                tokens.NextToken.TokenType == RdTokenType.CloseBrace &&
                tokens.LookAhead(2).TokenType == RdTokenType.OpenBrace &&
                tokens.LookAhead(3).TokenType == RdTokenType.Argument)
            {
                tokens.Advance(3);

                string suffix = context.TextProvider.GetText(tokens.CurrentToken).Trim();
                tokens.MoveToNextToken();

                if (tokens.CurrentToken.TokenType == RdTokenType.CloseBrace &&
                    tokens.NextToken.TokenType == RdTokenType.OpenBrace &&
                    tokens.LookAhead(2).TokenType == RdTokenType.Argument)
                {
                    tokens.Advance(2);
                    string signatureText = context.TextProvider.GetText(tokens.CurrentToken).Trim();
                    return ParseSignature(signatureText);
                }
            }

            return null;
        }

        private static SignatureInfo ParseSignature(string text)
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
            List<ArgumentInfo> signatureArguments = new List<ArgumentInfo>();

            var rParseContext = new Microsoft.R.Core.Parser.ParseContext(textProvider, new TextRange(0, text.Length), tokens);

            FunctionCall functionCall = new FunctionCall();
            functionCall.Parse(rParseContext, rParseContext.AstRoot);

            foreach (var arg in functionCall.Arguments)
            {
                ArgumentInfo argInfo = new ArgumentInfo();

                ExpressionArgument expArg = arg as ExpressionArgument;
                if (expArg != null)
                {
                    argInfo.Name = textProvider.GetText(expArg);
                }
                else
                {
                    NamedArgument nameArg = arg as NamedArgument;
                    if (nameArg != null)
                    {
                        argInfo.Name = textProvider.GetText(nameArg.NameRange);
                        argInfo.DefaultValue = textProvider.GetText(nameArg.ArgumentValue);
                    }
                    else
                    {
                        EllipsisArgument ellipsisArg = arg as EllipsisArgument;
                        if (ellipsisArg != null)
                        {
                            argInfo.Name = "...";
                            argInfo.IsOptional = true;
                        }
                    }
                }

                signatureArguments.Add(argInfo);
            }

            info.Arguments = signatureArguments;
            return info;
        }
    }
}
