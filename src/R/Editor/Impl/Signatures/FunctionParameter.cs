// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Describes parameter (actual argument) in a function call
    /// </summary>
    public sealed class FunctionParameter {
        /// <summary>
        /// Function call
        /// </summary>
        public FunctionCall FunctionCall { get; }

        /// <summary>
        /// Parameter index in the function call arguments
        /// </summary>
        public int ParameterIndex { get; }

        /// <summary>
        /// Parameter name if parameter is a named parameter
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// If true then the parameter is a named parameter in a function call
        /// </summary>
        public bool NamedParameter { get; }

        /// <summary>
        /// Function name
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Package name
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// Function signature end in the current text snapshot
        /// </summary>
        public int SignatureEnd => FunctionCall.SignatureEnd;

        public FunctionParameter(string packageName, string functionName, FunctionCall functionCall, int parameterIndex, string parameterName, bool namedParameter) {
            Check.ArgumentNull(nameof(functionName), functionName);
            Check.ArgumentNull(nameof(functionCall), functionCall);

            PackageName = packageName;
            FunctionName = functionName;
            FunctionCall = functionCall;
            ParameterIndex = parameterIndex;
            ParameterName = parameterName;
            NamedParameter = namedParameter;
        }

        public static FunctionParameter FromEditorBuffer(AstRoot astRoot, IEditorBufferSnapshot snapshot, int position) {
            FunctionCall functionCall;
            Variable functionVariable;
            int parameterIndex = -1;
            string parameterName;
            bool namedParameter = false;
            string packageName = null;

            if (!GetFunction(astRoot, ref position, out functionCall, out functionVariable)) {
                return null;
            }

            parameterIndex = functionCall.GetParameterIndex(position);
            parameterName = functionCall.GetParameterName(parameterIndex, out namedParameter);

            var op = functionVariable.Parent as Operator;
            if (op != null && op.OperatorType == OperatorType.Namespace) {
                var id = (op.LeftOperand as Variable)?.Identifier;
                packageName = id != null ? astRoot.TextProvider.GetText(id) : null;
            }

            if (!string.IsNullOrEmpty(functionVariable.Name) && functionCall != null && parameterIndex >= 0) {
                return new FunctionParameter(packageName, functionVariable.Name, functionCall, parameterIndex, parameterName, namedParameter);
            }

            return null;
        }

        private static bool GetFunction(AstRoot astRoot, ref int position, out FunctionCall functionCall, out Variable functionVariable) {
            // Note that we do not want just the deepest call since in abc(def()) 
            // when position is over 'def' we actually want signature help for 'abc' 
            // while simple depth search will find 'def'.            
            functionVariable = null;
            int p = position;
            functionCall = astRoot.GetSpecificNodeFromPosition<FunctionCall>(p, (x) => {
                var fc = x as FunctionCall;
                if (fc != null && fc.OpenBrace.End <= p) {
                    if (fc.CloseBrace != null) {
                        return p <= fc.CloseBrace.Start; // between ( and )
                    } else {
                        // Take into account incomplete argument lists line in 'func(a|'
                        return fc.Arguments.End == p;
                    }
                }
                return false;
            });

            if (functionCall == null && position > 0) {
                // Retry in case caret is at the very end of function signature
                // that does not have final close brace yet.
                functionCall = astRoot.GetNodeOfTypeFromPosition<FunctionCall>(position - 1, includeEnd: true);
                if (functionCall != null) {
                    // But if signature does have closing brace and caret
                    // is beyond it, we are really outside of the signature.
                    if (functionCall.CloseBrace != null && position >= functionCall.CloseBrace.End) {
                        return false;
                    }

                    if (position > functionCall.SignatureEnd) {
                        position = functionCall.SignatureEnd;
                    }
                }
            }

            if (functionCall != null && functionCall.Children.Count > 0) {
                functionVariable = functionCall.Children[0] as Variable;
                if (functionVariable == null) {
                    // Might be in a namespace
                    var op = functionCall.Children[0] as IOperator;
                    if (op != null && op.OperatorType == OperatorType.Namespace) {
                        functionVariable = op.RightOperand as Variable;
                    }
                }
                return functionVariable != null;
            }

            return false;
        }
    }
}
