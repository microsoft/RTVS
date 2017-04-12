// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.Signatures;

namespace Microsoft.R.Editor {
    public static class AstRootExtensions {
        public static IFunctionInfo GetUserFunctionInfo(this AstRoot ast, string functionName, int position) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            var v = scope?.FindFunctionDefinitionByName(functionName, position);
            var rf = v?.Value as RFunction;
            var fd = rf?.Value as IFunctionDefinition;
            return fd?.MakeFunctionInfo(functionName);
        }

        public static bool IsPositionInComment(this AstRoot ast, int position) {
            bool inComment = false;
            inComment = ast.Comments.GetItemsContainingInclusiveEnd(position).Count > 0;
            if (!inComment) {
                var snapshot = ast.TextProvider as IBufferSnapshot;
                if (snapshot != null) {
                    var line = snapshot.GetLineFromPosition(position);
                    position -= line.Start;
                    var tokens = (new RTokenizer()).Tokenize(line.GetText());
                    var token = tokens.FirstOrDefault(t => t.Contains(position) || t.End == position);
                    inComment = token != null && token.TokenType == RTokenType.Comment;
                }
            }
            return inComment;
        }

        /// <summary>
        /// Given position in a text buffer finds method name.
        /// </summary>
        public static string GetFunctionNameFromBuffer(this AstRoot astRoot, ref int position, out int signatureEnd) {
            FunctionCall functionCall;
            Variable functionVariable;

            signatureEnd = -1;

            if (GetFunction(astRoot, ref position, out functionCall, out functionVariable)) {
                signatureEnd = functionCall.End;
                return functionVariable.Name;
            }

            return null;
        }

        public static bool GetFunction(this AstRoot astRoot, ref int position, out FunctionCall functionCall, out Variable functionVariable) {
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

        /// <summary>
        /// Given position in a text buffer finds method name, 
        /// parameter index as well as where method signature ends.
        /// </summary>
        public static ParameterInfo GetParametersInfoFromBuffer(this AstRoot astRoot, IEditorBufferSnapshot snapshot, int position) {
            FunctionCall functionCall;
            Variable functionVariable;
            int parameterIndex = -1;
            string parameterName;
            bool namedParameter = false;
            string packageName = null;

            if (!astRoot.GetFunction(ref position, out functionCall, out functionVariable)) {
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

        /// <summary>
        /// Determines if position is in the argument name. Typically used to
        ///     a) suppress general intellisense when typing function arguments 
        ///         in a function/ definition such as in 'x &lt;- function(a|'
        ///     b) determine if completion list should contain argumet names
        ///        when user types inside function call.
        /// </summary>
        public static bool IsInFunctionArgumentName<T>(this AstRoot ast, int position) where T : class, IFunction {
            T funcDef = ast.GetNodeOfTypeFromPosition<T>(position);
            if (funcDef == null || funcDef.OpenBrace == null || funcDef.Arguments == null) {
                return false;
            }

            if (position < funcDef.OpenBrace.End || position >= funcDef.SignatureEnd) {
                return false;
            }

            int start = funcDef.OpenBrace.End;
            int end = funcDef.SignatureEnd;

            if (funcDef.Arguments.Count == 0 && position >= start && position <= end) {
                return true;
            }

            for (int i = 0; i < funcDef.Arguments.Count; i++) {
                CommaSeparatedItem csi = funcDef.Arguments[i];
                NamedArgument na = csi as NamedArgument;

                if (position < csi.Start) {
                    break;
                }

                end = csi.End;
                if (position >= start && position <= end) {
                    if (na == null) {
                        return true;
                    }

                    if (position <= na.EqualsSign.Start) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if position is in object member. Typically used
        /// to suppress general intellisense when typing data member 
        /// name such as 'mtcars$|'
        /// </summary>
        public static bool IsInObjectMemberName(this ITextProvider textProvider, int position) {
            if (position > 0) {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = textProvider[i];

                    if (ch == '$' || ch == '@') {
                        return true;
                    }

                    if (!RTokenizer.IsIdentifierCharacter(ch)) {
                        break;
                    }
                }
            }
            return false;
        }
    }
}
