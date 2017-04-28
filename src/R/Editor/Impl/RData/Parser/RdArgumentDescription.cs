// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.RData.Tokens;

namespace Microsoft.R.Editor.RData.Parser {
    static class RdArgumentDescription {
        /// <summary>
        /// Extracts argument names and descriptions from
        /// the RD '\arguments{...} construct
        /// </summary>
        public static IReadOnlyDictionary<string, string> ExtractArgumentDecriptions(RdParseContext context) {
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

            Dictionary<string, string> argumentDescriptions = new Dictionary<string, string>();
            TokenStream<RdToken> tokens = context.Tokens;

            // '\arguments{' is expected
            Debug.Assert(tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace);
            if (tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace) {
                // Move past '\arguments'
                tokens.MoveToNextToken();

                int startTokenIndex, endTokenIndex;
                if (RdParseUtility.GetKeywordArgumentBounds(tokens, out startTokenIndex, out endTokenIndex)) {
                    // Now that we know bounds of \arguments{...} go through 
                    // inner '\item' elements and fetch description and all
                    // argument names the description applies to.
                    //
                    // Example:
                    //
                    //    \item{start, param, eps, iter, print}{Arguments 
                    //    passed to \code{\link{ loglin} }.}
                    //
                    while (!tokens.IsEndOfStream() && tokens.Position < endTokenIndex) {
                        RdToken token = tokens.CurrentToken;

                        if (context.IsAtKeyword(@"\item")) {
                            IEnumerable<IArgumentInfo> args = ParseArgumentItem(context);
                            if (args == null) {
                                break;
                            }

                            foreach (var a in args) {
                                argumentDescriptions[a.Name] = a.Description;
                            }
                        } else {
                            tokens.MoveToNextToken();
                        }
                    }
                }

                tokens.Position = endTokenIndex;
            }

            return argumentDescriptions;
        }

        private static IEnumerable<IArgumentInfo> ParseArgumentItem(RdParseContext context) {
            List<IArgumentInfo> arguments = null;

            TokenStream<RdToken> tokens = context.Tokens;
            tokens.Advance(1);

            // Past '\item'. Inside { } we can find any number of '\dots' which are keywords.
            Debug.Assert(tokens.CurrentToken.TokenType == RdTokenType.OpenCurlyBrace);

            if (tokens.CurrentToken.TokenType == RdTokenType.OpenCurlyBrace) {
                int startTokenIndex, endTokenIndex;
                if (RdParseUtility.GetKeywordArgumentBounds(tokens, out startTokenIndex, out endTokenIndex)) {
                    TextRange range = TextRange.FromBounds(tokens[startTokenIndex].End, tokens[endTokenIndex].Start);
                    string argumentsText = context.TextProvider.GetText(range);

                    string[] argumentNames = argumentsText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    arguments = new List<IArgumentInfo>();

                    // Move past \item{}
                    tokens.Position = endTokenIndex + 1;
                    Debug.Assert(tokens.CurrentToken.TokenType == RdTokenType.OpenCurlyBrace);

                    if (tokens.CurrentToken.TokenType == RdTokenType.OpenCurlyBrace) {
                        string description = RdText.GetText(context);

                        foreach (string n in argumentNames) {
                            string name = n.Trim();
                            if (name == @"\dots") {
                                name = "...";
                            }

                            ArgumentInfo info = new ArgumentInfo(name, description.Trim());
                            arguments.Add(info);
                        }
                    }
                }
            }

            return arguments;
        }
    }
}
