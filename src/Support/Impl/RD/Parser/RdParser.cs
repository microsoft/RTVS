using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    /// <summary>
    /// Parser of the RD (R Documentation) file format. Primary usage 
    /// of the parser is the extraction of information on functions 
    /// and their parameters so we can show signature completion 
    /// and quick info in the VS editor.
    /// </summary>
    public static class RdParser
    {
        /// <summary>
        /// Given RD data and function name parses the data and
        /// creates structured information about the function.
        /// </summary>
        public static IFunctionInfo GetFunctionInfo(string functionName, string rdHelpData)
        {
            var tokenizer = new RdTokenizer();
            ITextProvider textProvider = new TextStream(rdHelpData);
            IReadOnlyTextRangeCollection<RdToken> tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            return ParseFunction(functionName, tokens, textProvider);
        }

        private static IFunctionInfo ParseFunction(string functionName, IReadOnlyTextRangeCollection<RdToken> tokens, ITextProvider textProvider)
        {
            RdParseContext context = new RdParseContext(tokens, textProvider);
            FunctionInfo info = new FunctionInfo(functionName);
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
                        argumentDescriptions = RdArgumentDescription.ExtractArgumentDecriptions(context);
                    }
                    else if (info.Signatures == null && token.IsKeywordText(textProvider, @"\usage"))
                    {
                        info.Signatures = RdFunctionSignature.ExtractSignatures(context);
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
                        if (argumentDescriptions.TryGetValue(arg.Name, out description))
                        {
                            ((NamedItemInfo)arg).Description = description ?? string.Empty;
                        }
                    }
                }
            }

            info.Aliases = aliases;
            return info;
        }
    }
}
