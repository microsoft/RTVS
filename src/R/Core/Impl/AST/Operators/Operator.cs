// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Operators {
    [DebuggerDisplay("[{OperatorType} [{Start}...{End})]")]
    public abstract class Operator : RValueNode<RObject>, IOperator {
        #region IOperator
        public IRValueNode LeftOperand { get; set; }

        public virtual OperatorType OperatorType { get; private set; }

        public IRValueNode RightOperand { get; set; }

        public virtual int Precedence { get; internal set; }

        public virtual bool IsUnary { get; private set; }

        public virtual Association Association { get; internal set; }
        #endregion

        private static bool IsPossibleUnary(OperatorType operatorType) {
            switch (operatorType) {
                case OperatorType.Tilde:
                case OperatorType.Not:
                case OperatorType.Add:
                case OperatorType.Subtract:
                case OperatorType.Help:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Given token stream and operator type determines if operator is unary
        /// </summary>
        /// <param name="tokens">Token stream</param>
        /// <param name="operatorType">Operator type</param>
        /// <param name="offset">Offset of the operator relatively to the current token position</param>
        public static bool IsUnaryOperator(TokenStream<RToken> tokens, OperatorType operatorType, int offset = 0) {
            bool possibleUnary = Operator.IsPossibleUnary(operatorType);

            if (!possibleUnary) {
                return false;
            }

            if (tokens.Position == offset) {
                // First operator in the expression is unary
                return true;
            }

            // If operator is preceded by an operator, it is then unary
            // Look back two tokens since operator parsing already consumed its token.
            if (tokens.LookAhead(offset - 1).TokenType == RTokenType.Operator) {
                return true;
            }

            return false;
        }
    }
}
