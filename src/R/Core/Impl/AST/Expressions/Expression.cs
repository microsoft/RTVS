// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Expressions {
    /// <summary>
    /// Represents mathematical or conditional expression, 
    /// assignment, function or operator definition optionally
    /// enclosed in braces. Expression is a tree and may have
    /// nested extressions in its content.
    /// </summary>
    [DebuggerDisplay("Expression [{Start}...{End})")]
    public sealed partial class Expression : RValueNode, IExpression {
        private readonly string _terminatingKeyword;

        #region IExpression
        public IRValueNode Content { get; internal set; }
        #endregion

        /// <summary>
        /// Constructs an expression that will be parsed as it is
        /// inside braces (in a group) so expression parsing
        /// will continue even if there is a line break
        /// that would normally terminate the expression.
        /// </summary>
        /// <param name="inGroup"></param>
        public Expression(bool inGroup) {
            IsInGroup = inGroup;
        }

        public Expression(string terminatingKeyword) {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            if (ParseExpression(context) && Children.Count > 0) {
                return base.Parse(context, parent);
            }

            return false;
        }

        public bool IsInGroup { get; private set; }

        public override string ToString() {
            var text = Root?.TextProvider.GetText(this);
            return !string.IsNullOrWhiteSpace(text) ? text : "Expression";
        }
    }
}
