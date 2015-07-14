using System;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Statements.Conditionals
{
    /// <summary>
    /// Branching ('if') statement
    /// http://cran.r-project.org/doc/manuals/r-release/R-lang.html#if
    /// </summary>
    public sealed class If : KeywordExpressionScopeStatement
    {
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
            base(_terminatingKeyword)
        {
        }

        public KeywordScopeStatement Else { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            // First parse base which should pick up keyword, braces, inner
            // expression and either full or simple (single statement) scope
            if (!base.Parse(context, parent))
            {
                return false;
            }

            // At this point we should be either at 'else' token or 
            // at the next statement. In the latter case we are done.
            TokenStream<RToken> tokens = context.Tokens;

            // If scope is a simple scope, then 'Else' must be on the same line
            // as the final token of the simple scope statement.
            bool isSimpleScope = this.Scope.OpenCurlyBrace == null;

            if (tokens.CurrentToken.IsKeywordText(context.TextProvider, "else"))
            {
                if (isSimpleScope)
                {
                    // Verify that there is no line break before the 'else'
                    if (context.Tokens.IsLineBreakAfter(context.TextProvider, tokens.Position - 1))
                    {
                        context.AddError(new ParseError(ParseErrorType.UnexpectedToken, ParseErrorLocation.Token, tokens.CurrentToken));
                        return false;
                    }
                }

                this.Else = new KeywordScopeStatement(allowsSimpleScope: true);
                return this.Else.Parse(context, this);
            }

            // Not at 'else' so we are done here
            return true;
        }
    }
}
