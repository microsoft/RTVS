// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments {
    /// <summary>
    /// Represents argument that has errors and 
    /// count not be completely parsed but argument parser
    /// was able to recover such as in 'func(a b, c, d)
    /// where first argument is an invalid expression.
    /// </summary>
    [DebuggerDisplay("Error Argument [{Start}...{End})")]
    public sealed class ErrorArgument : CommaSeparatedItem {
        /// <summary>
        /// Tokens between previous and the next comma (if any)
        /// </summary>
        public IReadOnlyTextRangeCollection<RToken> Tokens { get; private set; }

        public ErrorArgument(IEnumerable<RToken> tokens) {
            Tokens = new TextRangeCollection<RToken>(tokens);
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            foreach (var t in Tokens) {
                var n = new TokenNode(t) { Parent = this };
            }
            return base.Parse(context, parent);
        }
    }
}
