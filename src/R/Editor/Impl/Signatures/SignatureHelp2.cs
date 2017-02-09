// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Completion;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Completion;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Signatures {
    public partial class SignatureHelp {
        public static void TriggerSignatureHelp(ITextView textView, ICoreShell shell) {
            CompletionController.DismissSignatureSession(textView, shell);
            var rcc = RCompletionController.FromTextView(textView);
            // Since this call may come async via dispatcher the editor
            // window may be already closed at this point.
            rcc?.TriggerSignatureHelp();
        }

        public static void DismissSession(ITextView textView, ICoreShell shell, bool retrigger = false) {
            CompletionController.DismissSignatureSession(textView, shell);
            if (retrigger) {
                var rcc = RCompletionController.FromTextView(textView);
                // Since this call may come async via dispatcher the editor
                // window may be already closed at this point.
                rcc?.TriggerSignatureHelp();
            }
        }

        /// <summary>
        /// Given position in a text buffer finds method name.
        /// </summary>
        public static string GetFunctionNameFromBuffer(AstRoot astRoot, ref int position, out int signatureEnd) {
            FunctionCall functionCall;
            Variable functionVariable;

            signatureEnd = -1;

            if (GetFunction(astRoot, ref position, out functionCall, out functionVariable)) {
                signatureEnd = functionCall.End;
                return functionVariable.Name;
            }

            return null;
        }

        /// <summary>
        /// Given position in a text buffer finds method name, 
        /// parameter index as well as where method signature ends.
        /// </summary>
        public static ParameterInfo GetParametersInfoFromBuffer(AstRoot astRoot, ITextSnapshot snapshot, int position) {
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
                return new ParameterInfo(packageName, functionVariable.Name, functionCall, parameterIndex, parameterName, namedParameter);
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
