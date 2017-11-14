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
            return fd?.MakeFunctionInfo(functionName, false);
        }

        public static bool IsPositionInComment(this AstRoot ast, int position) {
            var inComment = ast.Comments.GetItemsContainingInclusiveEnd(position).Count > 0;
            if (!inComment) {
                var snapshot = (ast.TextProvider as IEditorBufferSnapshotProvider)?.Snapshot;
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
            signatureEnd = -1;
            if (GetOuterFunction(astRoot, ref position, out var functionCall, out var functionVariable)) {
                signatureEnd = functionCall.End;
                return functionVariable.Name;
            }
            return null;
        }

        /// <summary>
        /// Given position over function name retrieves name range and the function call.
        /// </summary>
        public static string GetFunctionName(this AstRoot ast, int position, out FunctionCall functionCall, out Variable functionVariable) {
            functionVariable = null;
            functionCall = null;

            ast.GetPositionNode(position, out var node);
            if (node == null) {
                return null;
            }

            // In abc(de|f(x)) first find inner function, then outer.
            if (node is TokenNode && node.Parent is FunctionCall) {
                functionCall = (FunctionCall)node.Parent;
            } else {
                functionCall = ast.GetNodeOfTypeFromPosition<FunctionCall>(position);
            }
            functionVariable = functionCall?.RightOperand as Variable;
            return functionVariable?.Name;
        }

        /// <summary>
        /// Finds the outermost function call from given position.
        /// In abc(def()) when position is over 'def' finds 'abc' 
        /// </summary>
        public static bool GetOuterFunction(this AstRoot astRoot, ref int position, out FunctionCall functionCall, out Variable functionVariable) {
            functionVariable = null;
            var p = position;
            functionCall = astRoot.GetSpecificNodeFromPosition<FunctionCall>(p, x => {
                if (x is FunctionCall fc && fc.OpenBrace.End <= p) {
                    if (fc.CloseBrace != null) {
                        return p <= fc.CloseBrace.Start; // between ( and )
                    }
                    // Take into account incomplete argument lists line in 'func(a|'
                    return fc.Arguments.End == p;
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
                    if (functionCall.Children[0] is IOperator op && op.OperatorType == OperatorType.Namespace) {
                        functionVariable = op.RightOperand as Variable;
                    }
                }
                return functionVariable != null;
            }

            return false;
        }

        /// <summary>
        /// Given position in a text buffer finds method name, 
        /// parameter index as well as where the method signature ends.
        /// </summary>
        public static RFunctionSignatureInfo GetSignatureInfoFromBuffer(this AstRoot ast, IEditorBufferSnapshot snapshot, int position) {
            // For signatures we want outer function so in abc(d|ef()) helps shows signature for the 'abc'.
            if (!ast.GetOuterFunction(ref position, out var functionCall, out var functionVariable)) {
                return null;
            }
            return GetSignatureInfo(ast, functionCall, functionVariable, position);
        }

        /// <summary>
        /// Given position in a text buffer and the function call, 
        /// parameter index as well as where the method signature ends.
        /// </summary>
        public static RFunctionSignatureInfo GetSignatureInfo(this AstRoot ast, FunctionCall functionCall, Variable functionVariable, int position) {
            var parameterIndex = functionCall.GetParameterIndex(position);
            var parameterName = functionCall.GetParameterName(parameterIndex, out var namedParameter);

            string packageName = null;
            if (functionVariable.Parent is Operator op && op.OperatorType == OperatorType.Namespace) {
                var id = (op.LeftOperand as Variable)?.Identifier;
                packageName = id != null ? ast.TextProvider.GetText(id) : null;
            }

            if (!string.IsNullOrEmpty(functionVariable.Name) && parameterIndex >= 0) {
                return new RFunctionSignatureInfo(packageName, functionVariable.Name, functionCall, parameterIndex, parameterName, namedParameter);
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
            var funcDef = ast.GetNodeOfTypeFromPosition<T>(position);
            if (funcDef?.OpenBrace == null || funcDef.Arguments == null) {
                return false;
            }

            if (position < funcDef.OpenBrace.End || position >= funcDef.SignatureEnd) {
                return false;
            }

            var start = funcDef.OpenBrace.End;
            var end = funcDef.SignatureEnd;

            if (funcDef.Arguments.Count == 0 && position >= start && position <= end) {
                return true;
            }

            foreach (var csi in funcDef.Arguments) {
                var na = csi as NamedArgument;

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
                for (var i = position - 1; i >= 0 && i < textProvider.Length; i--) {
                    var ch = textProvider[i];

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
