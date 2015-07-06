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
        public KeywordScopeStatement Else { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (base.Parse(context, parent))
            {
                TokenStream<RToken> tokens = context.Tokens;

                // In R any expression is permitted like in C/C++. It is not limited to conditiona
                // expressions like in C# where 'x = y' is not valid inside 'if' or 'while'.

                // http://cran.r-project.org/doc/manuals/r-release/R-lang.html#if
                // If value1 is a logical vector with first element TRUE then statement2 is evaluated. 
                // If the first element of value1 is FALSE then statement3 is evaluated. If value1 is 
                // a numeric vector then statement3 is evaluated when the first element of value1 is 
                // zero and otherwise statement2 is evaluated. Only the first element of value1 is used. 
                // All other elements are ignored. If value1 has any type other than a logical or 
                // a numeric vector an error is signalled.

                if (tokens.CurrentToken.TokenType == RTokenType.Keyword)
                {
                    string text = context.TextProvider.GetText(tokens.CurrentToken);
                    if (text.Equals("else", StringComparison.Ordinal))
                    {
                        this.Else = new KeywordScopeStatement(allowsSimpleScope: true);
                        if(this.Else.Parse(context, this))
                        {
                            return base.Parse(context, parent);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return base.Parse(context, parent);
                }
            }

            return false;
        }
    }
}
