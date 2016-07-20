// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Operands;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements.Conditionals {
    /// <summary>
    /// Branching ('if') statement
    /// http://cran.r-project.org/doc/manuals/r-release/R-lang.html#if
    /// </summary>
    [DebuggerDisplay("[If Statement]")]
    public class If : KeywordExpressionScopeStatement {
        private const string _terminatingKeyword = "else";

        // In R any expression is permitted like in C/C++. It is not limited to conditional
        // expressions like in C# where 'x = y' is not valid inside 'if' or 'while'.

        // http://cran.r-project.org/doc/manuals/r-release/R-lang.html#if
        // If value1 is a logical vector with first element TRUE then statement2 is evaluated. 
        // If the first element of value1 is FALSE then statement3 is evaluated. If value1 is 
        // a numeric vector then statement3 is evaluated when the first element of value1 is 
        // zero and otherwise statement2 is evaluated. Only the first element of value1 is used. 
        // All other elements are ignored. If value1 has any type other than a logical or 
        // a numeric vector an error is signalled.

        public If() :
            base(_terminatingKeyword) {
        }

        public KeywordScopeStatement Else { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent) {
            // First parse base which should pick up keyword, braces, inner
            // expression and either full or simple (single statement) scope
            if (!base.Parse(context, parent)) {
                return false;
            }

            // At this point we should be either at 'else' token or 
            // at the next statement. In the latter case we are done.
            TokenStream<RToken> tokens = context.Tokens;

            if (tokens.CurrentToken.IsKeywordText(context.TextProvider, "else")) {
                bool allowLineBreak = AllowLineBreakBeforeElse(context);
                if (!allowLineBreak) {
                    // Verify that there is no line break before the 'else'
                    if (context.Tokens.IsLineBreakAfter(context.TextProvider, tokens.Position - 1)) {
                        context.AddError(new ParseError(ParseErrorType.UnexpectedToken, ErrorLocation.Token, tokens.CurrentToken));
                        return true;
                    }
                }
                this.Else = new KeywordScopeStatement(allowsSimpleScope: true);
                return this.Else.Parse(context, this);
            }

            // Not at 'else' so we are done here
            return true;
        }

        private bool AllowLineBreakBeforeElse(ParseContext context) {
            if (context.Scopes.Count > 1) {
                return true;
            }

            // The problem here is that in recursive descent parser
            // node parent hasn't been assigned yet since it is assigned
            // when construct is fully parsed. So we will check if this
            // 'if' is part of an ExpressionStatement (which is in the
            // global scope. 
            //
            // Variants:
            //      a. If is not an 'inline if' and rather is a statement by itself.
            //      b. If is 'inline if' but is a part of an expression
            //         that is inside expression statement and has no braces.
            //

            if (!(this is InlineIf)) {
                // 'if' statement
                return false;
            }

            // Now we need to check if the expression being parsed.
            // is in a Group i.e. enclosed in ( ).
            Debug.Assert(context.Expressions.Count > 0);
            Expression expression = context.Expressions.Count > 0 ? context.Expressions.Peek() : null;
            if (expression != null && expression.IsInGroup) {
                return true;
            }

            // Remaining case: x <- a + if(y < 0) 1
            return false;
        }
    }
}
