using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments
{
    [DebuggerDisplay("[{Count}]")]
    public abstract class CommaSeparatedList : AstNode
    {
        private List<IAstNode> _arguments = new List<IAstNode>(1);
        private RTokenType _terminatingTokenType;
        private bool _missingLastArgument;

        public CommaSeparatedList(RTokenType terminatingTokenType)
        {
            _terminatingTokenType = terminatingTokenType;
        }

        protected abstract IAstNode CreateItem(IAstNode parent, ParseContext context);

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            bool result = true;

            while (!context.Tokens.IsEndOfStream())
            {
                if (context.Tokens.CurrentToken.TokenType == _terminatingTokenType)
                {
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
                    if (result)
                    {
                        _arguments.Add(item);
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

            // Handle final missing argument as in abc(,,) or abc(,,
            // Note that the missing argument has same text position as 
            // the closing brace of end of stream which presents a problem
            // since all items must have distinct positions in the buffer.
            // So we don't actually add item to the tree but instead cook up
            // different count of arguments and handle enumeration properly.
            if (context.Tokens.PreviousToken.TokenType == RTokenType.Comma &&
                (context.Tokens.CurrentToken.TokenType == _terminatingTokenType || context.Tokens.IsEndOfStream()))
            {
                _missingLastArgument = true;
            }

            if (result && Children.Count > 0)
            {
                base.Parse(context, parent);
            }

            return result;
        }

        public int Count
        {
            get { return _missingLastArgument ? _arguments.Count + 1 : _arguments.Count; }
        }

        public IAstNode this[int i]
        {
            get
            {
                if (_missingLastArgument && i == _arguments.Count)
                {
                    return new MissingArgument();
                }

                return _arguments[i];
            }
        }
    }
}
