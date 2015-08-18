using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.RD.Parser
{
    static class RdArgumentDescription
    {
        /// <summary>
        /// Extracts argument names and descriptions from
        /// the RD '\arguments{...} construct
        /// </summary>
        public static IReadOnlyDictionary<string, string> ExtractArgumentDecriptions(RdParseContext context)
        {
            // \arguments{
            //   \item{formula}{
            //       A linear model formula specifying the log - linear model.
            //       See \code{\link{ loglm} } for its interpretation.
            //   }
            //   \item{data}{
            //       Numeric array or data frame.In the first case it specifies the
            //       array of frequencies; in then second it provides the data frame
            //       from which the variables occurring in the formula are
            //       preferentially obtained in the usual way.
            //   }
            //   \item{start, param, eps, iter, print}{
            //       Arguments passed to \code{\link{ loglin} }.
            //   }
            //   \item{\dots}{ 
            //       arguments passed to the default method.
            //   }
            // }

            TokenStream<RdToken> tokens = context.Tokens;

            // '\arguments{' is expected
            if (tokens.NextToken.TokenType != RdTokenType.OpenBrace)
            {
                return null;
            }

            tokens.MoveToNextToken();
            Dictionary<string, string> argumentDescriptions = new Dictionary<string, string>();

            int start = tokens.Position;
            int end = RdParseUtility.FindRdKeywordArgumentBounds(tokens);

            // Now that we know bounds of \arguments{...} go through 
            // inner '\item' elements and fetch description and all
            // argument names the description applies to.
            //
            // Example:
            //
            //    \item{start, param, eps, iter, print}{Arguments 
            //    passed to \code{\link{ loglin} }.}
            //
            while (!tokens.IsEndOfStream() && tokens.Position < end)
            {
                RdToken token = tokens.CurrentToken;

                if (token.IsKeywordText(context.TextProvider, @"\item") && tokens.NextToken.TokenType == RdTokenType.OpenBrace)
                {
                    IEnumerable<ArgumentInfo> args = ParseArgumentItem(context);
                    if (args == null)
                    {
                        tokens.Position = end;
                        return null;
                    }

                    foreach(var a in args)
                    {
                        argumentDescriptions[a.Name] = a.Description;
                    }
                }
                else
                {
                    tokens.MoveToNextToken();
                }
            }

            return argumentDescriptions;
        }

        private static IEnumerable<IArgumentInfo> ParseArgumentItem(RdParseContext context)
        {
            List<IArgumentInfo> arguments = null;

            TokenStream<RdToken> tokens = context.Tokens;
            tokens.Advance(2);

            if (tokens.CurrentToken.TokenType == RdTokenType.Argument && tokens.NextToken.TokenType == RdTokenType.CloseBrace)
            {
                arguments = new List<IArgumentInfo>();

                string argumentsText = context.TextProvider.GetText(tokens.CurrentToken);
                string[] argumentNames = argumentsText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                tokens.Advance(2);

                if (tokens.CurrentToken.TokenType == RdTokenType.OpenBrace)
                {
                    string description = RdText.GetText(context);

                    foreach (string name in argumentNames)
                    {
                        ArgumentInfo info = new ArgumentInfo();

                        info.Name = name.Trim();
                        info.Description = description.Trim();

                        arguments.Add(info);
                    }
                }
            }

            return arguments;
        }
    }
}
