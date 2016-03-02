// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Variables {
    /// <summary>
    /// Braces (grouping) operator. Applies to an expression
    /// as in (a+b). Operator is effective a no-op and 
    /// returns value of the expression inside braces.
    /// It makes parsing expressions like (b)[] easier.
    /// </summary>
    [DebuggerDisplay("Grouping [{Start}...{End})")]
    public sealed class Group : Operator {
        public TokenNode OpenBrace { get; private set; }
        public IExpression Content { get; private set; }
        public TokenNode CloseBrace { get; private set; }

        #region IOperator
        public override bool IsUnary {
            get { return true; }
        }

        public override OperatorType OperatorType {
            get { return OperatorType.Group; }
        }

        public override int Precedence {
            get { return OperatorPrecedence.GetPrecedence(OperatorType.Group); }
        }

        public override Association Association {
            get { return Association.Right; }
        }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenBrace);
            this.OpenBrace = RParser.ParseToken(context, this);

            this.Content = new Expression(inGroup: true);
            this.Content.Parse(context, this);

            if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace) {
                this.CloseBrace = RParser.ParseToken(context, this);
            } else {
                context.AddError(new MissingItemParseError(ParseErrorType.CloseBraceExpected, tokens.PreviousToken));
            }

            return base.Parse(context, parent);
        }
    }
}
