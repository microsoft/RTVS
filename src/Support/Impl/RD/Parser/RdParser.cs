using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    public static class RdParser
    {
        public static RdFunctionInfo GetFunctionInfo(string name, string rdHelpData)
        {
            var tokenizer = new RdTokenizer();
            ITextProvider textProvider = new TextStream(rdHelpData);
            IReadOnlyTextRangeCollection<RdToken> tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            return ParseFunction(name, tokens, textProvider);
        }

        public static RdFunctionInfo ParseFunction(string name, IReadOnlyTextRangeCollection<RdToken> tokens, ITextProvider textProvider)
        {
            ParseContext context = new ParseContext(tokens, textProvider);
            RdFunctionInfo info = new RdFunctionInfo(name);
            List<string> aliases = new List<string>();
            IReadOnlyDictionary<string, string> argumentDescriptions = null;

            while (!context.Tokens.IsEndOfStream() && !info.IsComplete && argumentDescriptions == null)
            {
                RdToken token = context.Tokens.CurrentToken;

                if (token.TokenType == RdTokenType.Keyword &&
                    context.Tokens.NextToken.TokenType == RdTokenType.OpenBrace)
                {
                    if (string.IsNullOrEmpty(info.Description) && token.IsKeywordText(textProvider, @"\description"))
                    {
                        info.Description = RdText.GetText(context);
                    }
                    else if (token.IsKeywordText(textProvider, @"\alias"))
                    {
                        string alias = RdText.GetText(context);
                        if (!string.IsNullOrEmpty(alias))
                        {
                            aliases.Add(alias);
                        }
                    }
                    else if (token.IsKeywordText(textProvider, @"\keyword"))
                    {
                        string keyword = RdText.GetText(context);
                        if (!string.IsNullOrEmpty(keyword) && keyword.Contains("internal"))
                        {
                            info.IsInternal = true;
                        }
                    }
                    else if (string.IsNullOrEmpty(info.ReturnValue) && token.IsKeywordText(textProvider, @"\value"))
                    {
                        info.ReturnValue = RdText.GetText(context);
                    }
                    else if (argumentDescriptions == null && token.IsKeywordText(textProvider, @"\arguments"))
                    {
                        argumentDescriptions = FunctionArgumentDescriptions.ExtractArgumentDecriptions(context);
                    }
                    else if (info.Signatures == null && token.IsKeywordText(textProvider, @"\usage"))
                    {
                        info.Signatures = FunctionSignature.ExtractSignatures(context);
                    }
                    else
                    {
                        context.Tokens.Advance(2);
                    }
                }

                if (context.Tokens.CurrentToken.TokenType != RdTokenType.Keyword)
                {
                    context.Tokens.MoveToNextToken();
                }
            }

            if (argumentDescriptions != null)
            {
                // Merge descriptions into signatures
                foreach (SignatureInfo sigInfo in info.Signatures)
                {
                    foreach (var arg in sigInfo.Arguments)
                    {
                        string description;
                        argumentDescriptions.TryGetValue(arg.Name, out description);
                        arg.Description = description ?? string.Empty;
                    }
                }
            }

            info.Aliases = aliases;
            return (info.Signatures != null && info.Signatures.Count > 0) ? info : null;
        }
    }
}
