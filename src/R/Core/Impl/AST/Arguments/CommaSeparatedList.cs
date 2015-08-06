using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments
{
    [DebuggerDisplay("[{Count}]")]
    public abstract class CommaSeparatedList: AstNode, IReadOnlyCollection<IAstNode>
    {
        private List<IAstNode> arguments = new List<IAstNode>(1);
        private RTokenType terminatingTokenType;

        public CommaSeparatedList(RTokenType terminatingTokenType)
        {
            this.terminatingTokenType = terminatingTokenType;
        }

        protected abstract IAstNode CreateItem(IAstNode parent, ParseContext context);

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            bool result = true;

            while (!context.Tokens.IsEndOfStream())
            {
                if (context.Tokens.CurrentToken.TokenType == this.terminatingTokenType)
                {
                    if(context.Tokens.PreviousToken.TokenType == RTokenType.Comma)
                    {
                        // In x[2,] final missing argument is NOT added
                        // to the tree since it has no text position.

                        //MissingArgument ma = new MissingArgument();
                        //ma.Parse(context, this);
                        //this.arguments.Add(ma);
                    }

                    break;
                }

                if (RParser.IsScopeSeparator(context.Tokens.CurrentToken.TokenType))
                {
                    return false;
                }

                IAstNode item = this.CreateItem(this, context);
                if (item != null)
                {
                    result = item.Parse(context, this);
                    if(result)
                    {
                        this.arguments.Add(item);
                    }
                    else
                    {
                        // Try to recoved at comma or closing brace so
                        // we can detect all errors in the argument list
                        // and  not just the first one.
                        if (context.Tokens.CurrentToken.TokenType != RTokenType.Comma &&
                            context.Tokens.CurrentToken.TokenType != RTokenType.CloseBrace)
                        {
                            break;
                        }
                    }
                }
            }

            if(result && Children.Count > 0)
            {
                base.Parse(context, parent);
            }

            return result;
        }

        #region IReadOnlyCollection
        public int Count
        {
            get { return this.arguments.Count; }
        }

        public IEnumerator<IAstNode> GetEnumerator()
        {
            return this.arguments.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.arguments.GetEnumerator();
        }
        #endregion
    }
}
