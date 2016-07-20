// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents statement: assignment, function definition,
    /// function call, conditional statement and so on.
    /// </summary>
    [DebuggerDisplay("Statement, Children: {Children.Count}")]
    public abstract class Statement : AstNode, IStatement {
        /// <summary>
        /// Optional terminating semicolon
        /// </summary>
        public TokenNode Semicolon { get; private set; }

        protected bool ParseSemicolon(ParseContext context, IAstNode parent) {
            if (!context.Tokens.IsEndOfStream()) {
                if (!context.Tokens.IsLineBreakAfter(context.TextProvider, context.Tokens.Position - 1)) {
                    if (context.Tokens.CurrentToken.TokenType == RTokenType.Semicolon) {
                        this.Semicolon = RParser.ParseToken(context, this);
                    } else {
                        context.AddError(new ParseError(ParseErrorType.UnexpectedToken, ErrorLocation.Token, context.Tokens.CurrentToken));
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Abstract factory creating statements depending on current
        /// token and the following token sequence
        /// </summary>
        /// <returns></returns>
        public static IStatement Create(ParseContext context, IAstNode parent, string terminatingKeyword) {
            TokenStream<RToken> tokens = context.Tokens;
            RToken currentToken = tokens.CurrentToken;

            IStatement statement = null;

            switch (currentToken.TokenType) {
                case RTokenType.Keyword:
                    // If statement starts with a keyword, it is not an assignment
                    // hence we should always try keyword based statements first.
                    // Some of the statements may be R-values like typeof() but
                    // in case of the statement appearing on its own return value
                    // will be simply ignored. IDE may choose to show a warning.
                    if (currentToken.SubType == RTokenSubType.BuiltinFunction && tokens.NextToken.TokenType != RTokenType.OpenBrace) {
                        // 'return <- x + y' is allowed
                        statement = new ExpressionStatement(terminatingKeyword);
                    } else {
                        statement = KeywordStatement.CreateStatement(context, parent);
                    }
                    break;

                case RTokenType.Semicolon:
                    statement = new EmptyStatement();
                    break;

                default:
                    // Possible L-value in a left-hand assignment, 
                    // a function call or R-value in a right hand assignment.
                    statement = new ExpressionStatement(terminatingKeyword);
                    break;
            }

            return statement;
        }

        public override string ToString() {
            if (this.Semicolon != null && this.Root != null) {
                return this.Root.TextProvider.GetText(this.Semicolon);
            }

            return string.Empty;
        }
    }
}
