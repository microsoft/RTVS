// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST {
    /// <summary>
    /// The simplest type of AST tree item representing a single token.
    /// This item does not have children. Examples are identifiers,
    /// constants, operators, etc. All leaf nodes MUST be a token items.
    /// </summary>
    [DebuggerDisplay("[{Token.TokenType} : {Start}...{End}), Length = {Length}]")]
    public class TokenNode : AstNode {
        public RToken Token { get; protected set; }

        internal TokenNode() { }

        internal TokenNode(RToken token) {
            Token = token;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            RToken currentToken = context.Tokens.CurrentToken;

            this.Token = currentToken;
            context.Tokens.MoveToNextToken();

            return base.Parse(context, parent);
        }

        #region ITextRange
        public override int Start {
            get { return this.Token.Start; }
        }

        public override int End {
            get { return this.Token.End; }
        }

        public override IReadOnlyTextRangeCollection<IAstNode> Children {
            get { return TextRangeCollection<IAstNode>.EmptyCollection; }
        }

        public override bool Contains(int position) {
            return this.Token.Contains(position);
        }

        public override void Shift(int offset) {
            this.Token.Shift(offset);
        }

        public override void ShiftStartingFrom(int position, int offset) {
            if (this.Token.Start < position && position < this.Token.End) {
                // Leaf nodes are not composite range so we cannot shift parts.
                // Instead, we will expoand the range and next parsing pass
                // will generate actual new tokens
                this.Token.Expand(0, offset);
            } else if (position <= this.Token.Start) {
                this.Token.Shift(offset);
            }
        }
        #endregion

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            string name = (this.Root != null) ?
                this.Root.TextProvider.GetText(this.Token) : "<???>";

            sb.Append(name);
            sb.Append(" [");
            sb.Append(this.Start);
            sb.Append("...");
            sb.Append(this.End);
            sb.Append(')');

            return sb.ToString();
        }
    }
}
