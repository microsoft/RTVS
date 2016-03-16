// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments {
    [DebuggerDisplay("Named Argument [{Start}...{End})")]
    public class NamedArgument : CommaSeparatedItem {
        public ITextRange NameRange {
            get { return this.Identifier; }
        }
        public string Name {
            get { return this.Root.TextProvider.GetText(this.NameRange); }
        }

        public TokenNode Identifier { get; private set; }

        public TokenNode EqualsSign { get; private set; }

        public IExpression DefaultValue { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(context.Tokens.CurrentToken.TokenType == RTokenType.Identifier ||
                         context.Tokens.CurrentToken.TokenType == RTokenType.String);

            this.Identifier = RParser.ParseToken(context, this);
            this.EqualsSign = RParser.ParseToken(context, this);

            if (context.Tokens.CurrentToken.TokenType != RTokenType.Comma && context.Tokens.CurrentToken.TokenType != RTokenType.CloseBrace) {
                Expression exp = new Expression(inGroup: true);
                if (exp.Parse(context, this)) {
                    this.DefaultValue = exp;
                }
            } else {
                this.DefaultValue = new NullExpression();
            }

            return base.Parse(context, parent);
        }
    }
}
