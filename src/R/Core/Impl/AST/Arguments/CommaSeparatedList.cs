// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments {
    /// <summary>
    /// Comma separated list of expressions.
    /// Allows for missing values. Examples 
    /// are 'a, b+1, c[3]' or '1,,3.
    /// </summary>
    [DebuggerDisplay("Arguments: {Count} [{Start}...{End})")]
    public abstract class CommaSeparatedList : AstNode, IReadOnlyList<CommaSeparatedItem> {
        private List<CommaSeparatedItem> _arguments = new List<CommaSeparatedItem>();
        private RTokenType _terminatingTokenType;

        public CommaSeparatedList(RTokenType terminatingTokenType) {
            _terminatingTokenType = terminatingTokenType;
        }

        protected abstract CommaSeparatedItem CreateItem(IAstNode parent, ParseContext context);

        public override bool Parse(ParseContext context, IAstNode parent) {
            while (!context.Tokens.IsEndOfStream()) {
                if (context.Tokens.CurrentToken.TokenType == _terminatingTokenType) {
                    break;
                }

                if (RParser.IsListTerminator(context, RParser.GetOpeningTokenType(_terminatingTokenType), context.Tokens.CurrentToken)) {
                    AddStubArgument(context);
                    break;
                }

                var item = this.CreateItem(this, context);
                if (item != null) {
                    var currentPosition = context.Tokens.Position;
                    if (item.Parse(context, this)) {
                        _arguments.Add(item);
                    } else {
                        // Item could not be parser. We attempt to recover by
                        // walking forward and finding the nearest comma
                        // that can be used as a terminator for this argument.
                        if (!AddErrorArgument(context, currentPosition)) {
                            // Failed to figure out the recovery point
                            break;
                        }
                    }
                } else {
                    AddStubArgument(context);
                    break; // unexpected item
                }
            }

            if (_arguments.Count > 0 && _arguments[_arguments.Count - 1].Comma != null) {
                AddStubArgument(context);
            }

            // Do not include empty list in the tree since
            // it has no positioning information.
            if (_arguments.Count > 1 || (_arguments.Count == 1 && !(_arguments[0] is StubArgument))) {
                return base.Parse(context, parent);
            }

            return true;
        }

        #region ITextRange
        public override int End {
            get {
                // Exclude stub argument from range calculations
                if (_arguments.Count > 1 && (_arguments[_arguments.Count - 1] is StubArgument)) {
                    return _arguments[_arguments.Count - 2].End;
                }

                return base.End;
            }
        }
        #endregion

        private bool AddErrorArgument(ParseContext context, int startPosition) {
            var terminatorIndex = TryFindNextArgument(context, startPosition);
            if (terminatorIndex >= 0 && startPosition < terminatorIndex) {
                var tokens = new List<RToken>();
                for (var i = startPosition; i < terminatorIndex; i++) {
                    tokens.Add(context.Tokens[i]);
                }

                context.Tokens.Position = terminatorIndex;
                var arg = new ErrorArgument(tokens);
                arg.Parse(context, this);

                _arguments.Add(arg);
                return true;
            }

            return false;
        }

        private int TryFindNextArgument(ParseContext context, int startPosition) {
            for (var i = startPosition; i < context.Tokens.Length; i++) {
                var token = context.Tokens[i];
                if (token.TokenType == RTokenType.Comma || token.TokenType == RTokenType.CloseBrace) {
                    return i;
                }

                if (RParser.IsListTerminator(context, RTokenType.OpenBrace, token)) {
                    break;
                }
            }

            return -1;
        }

        /// <summary>
        /// In case of func( or func(,, adds final 'stub' argument
        /// since it does not have any associated token. 
        /// </summary>
        private void AddStubArgument(ParseContext context) {
            var arg = new StubArgument();
            _arguments.Add(arg);
        }

        #region IReadOnlyList<CommaSeparatedItem>
        /// <summary>
        /// Number of items in the list
        /// </summary>
        public int Count => _arguments.Count;
        public CommaSeparatedItem this[int i] => _arguments[i];
        public IEnumerator<CommaSeparatedItem> GetEnumerator() => _arguments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _arguments.GetEnumerator();
        #endregion
    }
}
