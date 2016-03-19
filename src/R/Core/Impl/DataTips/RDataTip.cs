// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.DataTips {
    public static class RDataTip {
        /// <summary>
        /// Given an R AST, and a range in that AST, determines the DataTip expression for that range.
        /// </summary>
        /// <returns>
        /// AST node corresponding to the DataTip expression, or <c>null</c> if there is no valid DataTip for the given range.
        /// </returns>
        /// <remarks>
        /// DataTip expression is defined as the outermost expression enclosing the range, such that the sequence 
        /// of operators from that outermost expression to the innermost node still enclosing the range only consists
        /// of operators: <c>$ @ :: ::: [ [[</c>. Furthermore, in case of <c>[ [[</c>, only their left operands
        /// are considered.
        /// </remarks>
        public static IAstNode GetDataTipExpression(IAstNode ast, ITextRange range) {
            var node = ast.NodeFromRange(range);
            if (node == null) {
                return null;
            }

            // DataTip can only be triggered by hovering over a literal, an identifier, or one of the operators: $ @ :: ::: [ [[
            var opNode = node as Operator;
            if (opNode != null) {
                switch (opNode.OperatorType) {
                    case OperatorType.Index:
                    case OperatorType.ListIndex:
                    case OperatorType.Namespace:
                        break;
                    default:
                        return null;
                }
            } else if (!(node is ILiteralNode || node is Variable)) {
                return null;
            }

            // Walk the AST tree up to determine the expression that should be evaluated, according to the following rules:
            // - if current node is a child of $, @, :: or ::: (on either side of the operator), keep walking;
            // - if current node is a left child of [ or [[, keep walking;
            // - otherwise, stop.
            // Thus, for an expression like a::b$c@d, the entire expression will be considered when hovering over a, b, c or d.
            // But for a[b$c][[d]], hovering over a will consider a[b$c][d], but hovering over b or c will only consider b$c.
            while (true) {
                var parent = node.Parent as Operator;
                if (parent == null) {
                    break;
                }

                var op = parent.OperatorType;
                if (op == OperatorType.Index) {
                    if (parent.LeftOperand != node) {
                        break;
                    }
                } else if (op != OperatorType.ListIndex && op != OperatorType.Namespace) {
                    break;
                }

                node = parent;
            }

            return node;
        }
    }
}
