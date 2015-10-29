using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser {
    /// <summary>
    /// Parser of the RD (R Documentation) file format. Primary usage 
    /// of the parser is the extraction of information on functions 
    /// and their parameters so we can show signature completion 
    /// and quick info in the VS editor.
    /// </summary>
    public static class RdParser {
        /// <summary>
        /// Given RD data and function name parses the data and
        /// creates structured information about the function.
        /// </summary>
        public static IFunctionInfo GetFunctionInfo(string functionName, string rdHelpData) {
            var tokenizer = new RdTokenizer(tokenizeRContent: false);

            ITextProvider textProvider = new TextStream(rdHelpData);
            IReadOnlyTextRangeCollection<RdToken> tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            RdParseContext context = new RdParseContext(tokens, textProvider);

            return ParseFunction(functionName, context);
        }

        private static IFunctionInfo ParseFunction(string functionName, RdParseContext context) {
            FunctionInfo info = new FunctionInfo(functionName);
            List<string> aliases = new List<string>();
            IReadOnlyDictionary<string, string> argumentDescriptions = null;

            while (!context.Tokens.IsEndOfStream() && argumentDescriptions == null) {
                RdToken token = context.Tokens.CurrentToken;

                if (context.IsAtKeywordWithParameters()) {
                    if (string.IsNullOrEmpty(info.Description) && context.IsAtKeyword(@"\description")) {
                        info.Description = RdText.GetText(context);
                    } else if (context.IsAtKeyword(@"\alias")) {
                        string alias = RdText.GetText(context);
                        if (!string.IsNullOrEmpty(alias)) {
                            aliases.Add(alias);
                        }
                    } else if (context.IsAtKeyword(@"\keyword")) {
                        string keyword = RdText.GetText(context);
                        if (!string.IsNullOrEmpty(keyword) && keyword.Contains("internal")) {
                            info.IsInternal = true;
                        }
                    } else if (string.IsNullOrEmpty(info.ReturnValue) && context.IsAtKeyword(@"\value")) {
                        info.ReturnValue = RdText.GetText(context);
                    } else if (argumentDescriptions == null && context.IsAtKeyword(@"\arguments")) {
                        argumentDescriptions = RdArgumentDescription.ExtractArgumentDecriptions(context);
                    } else if (info.Signatures == null && context.IsAtKeyword(@"\usage")) {
                        info.Signatures = RdFunctionSignature.ExtractSignatures(context);
                    } else {
                        context.Tokens.Advance(2);
                    }
                } else {
                    context.Tokens.MoveToNextToken();
                }
            }

            if (argumentDescriptions != null && info.Signatures != null) {
                // Merge descriptions into signatures
                foreach (SignatureInfo sigInfo in info.Signatures) {
                    foreach (var arg in sigInfo.Arguments) {
                        string description;
                        if (argumentDescriptions.TryGetValue(arg.Name, out description)) {
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
