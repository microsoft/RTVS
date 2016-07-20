// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Operators {
    /// <summary>
    /// Function call operator. Applies to a variable
    /// if it is a direct call like name() or to result
    /// of another similar operator such as indexer
    /// or function call as in x[1](a, b) or func(a)(b)(c).
    /// </summary>
    [DebuggerDisplay("FunctionCall, Args:{Arguments.Count} [{Start}...{End})")]
    public class FunctionCall : Operator, IFunction {
        /// <summary>
        /// 'virtual end' of the function call if closing
        /// brace is missing. The virtual end includes range
        /// after the last argument and up to the next recovery
        /// point such as curly brace, a keyword of end of the file.
        /// </summary>
        private int _virtualEnd;

        #region IFunction
        /// <summary>
        /// Opening brace. Always present.
        /// </summary>
        public TokenNode OpenBrace { get; private set; }

        /// <summary>
        /// Function arguments
        /// </summary>
        public ArgumentList Arguments { get; private set; }

        /// <summary>
        /// Closing brace. May be null if closing brace is missing.
        /// </summary>
        public TokenNode CloseBrace { get; private set; }

        /// <summary>
        /// Returns end of a function signature (list of arguments).
        /// In case closing brace is missing scope extends to a
        /// nearest recovery point which may be an identifier
        /// or a keyword (except inline 'if').
        /// </summary>
        public int SignatureEnd {
            get {
                if (CloseBrace != null) {
                    return CloseBrace.End;
                }

                Debug.Assert(_virtualEnd > 0);
                return _virtualEnd;
            }
        }
        #endregion

        #region IOperator
        public override OperatorType OperatorType  => OperatorType.FunctionCall;
        #endregion

        #region ITextRange
        public override int End {
            get { return SignatureEnd; }
        }
        #endregion

        public FunctionCall() {
            IsUnary = true;
        }

        public int GetParameterIndex(int position) {
            if (position > End || position < OpenBrace.End) {
                return -1;
            }

            if (Arguments.Count == 0) {
                return 0;
            }

            for (int i = 0; i < Arguments.Count; i++) {
                IAstNode arg = Arguments[i];
                if (arg is ErrorArgument) {
                    continue;
                }

                if (position < arg.End || (arg is MissingArgument && arg.Start == arg.End && arg.Start == 0)) {
                    return i;
                }
            }

            return Arguments.Count - 1;
        }

        /// <summary>
        /// Finds parameter name by parameter index and determines
        /// if the parameter is a named argument.
        /// </summary>
        /// <param name="index">Parameter index</param>
        /// <param name="namedParameter">If true, parameter is a named argument</param>
        /// <returns></returns>
        public string GetParameterName(int index, out bool namedParameter) {
            namedParameter = false;
            if (index < 0 || index > Arguments.Count - 1) {
                return string.Empty;
            }

            CommaSeparatedItem arg = Arguments[index];
            if (arg is NamedArgument) {
                namedParameter = true;
                return ((NamedArgument)arg).Name;
            } else if (arg is ExpressionArgument) {
                IExpression exp = ((ExpressionArgument)arg).ArgumentValue;
                if (exp.Children.Count == 1 && exp.Children[0] is Variable) {
                    return ((Variable)exp.Children[0]).Name;
                }
            }

            return string.Empty;
        }

        public override bool Parse(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.OpenBrace);
            this.OpenBrace = RParser.ParseToken(context, this);

            this.Arguments = new ArgumentList(RTokenType.CloseBrace);
            bool argumentsParsed = this.Arguments.Parse(context, this);
            if (argumentsParsed) {
                if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace) {
                    this.CloseBrace = RParser.ParseToken(context, this);
                }
            }

            if (!argumentsParsed || this.CloseBrace == null) {
                CalculateVirtualEnd(context);
            }

            if (this.CloseBrace == null) {
                context.AddError(new MissingItemParseError(ParseErrorType.CloseBraceExpected, tokens.PreviousToken));
            }

            return base.Parse(context, parent);
        }

        private void CalculateVirtualEnd(ParseContext context) {
            int position = this.Arguments.Count > 0 ? this.Arguments.End : this.OpenBrace.End;
            if (!context.Tokens.IsEndOfStream()) {
                TokenStream<RToken> tokens = context.Tokens;

                // Walk through tokens allowing numbers, identifiers and operators
                // as part of the function signature. Stop at keywords (except 'if'),
                // or curly braces.
                int i = tokens.Position;
                _virtualEnd = 0;

                for (; i < tokens.Length; i++) {
                    RToken token = tokens[i];

                    if (token.TokenType == RTokenType.Keyword || RParser.IsListTerminator(context, RTokenType.OpenBrace, token)) {
                        _virtualEnd = token.Start;
                        break;
                    }
                }
            }

            if (_virtualEnd == 0) {
                _virtualEnd = context.TextProvider.Length;
            }
        }

        #region ITextRange
        public override void Shift(int offset) {
            _virtualEnd += offset;
            base.Shift(offset);
        }
        public override void ShiftStartingFrom(int position, int offset) {
            _virtualEnd += offset;
            base.ShiftStartingFrom(position, offset);
        }
        #endregion
    }
}
