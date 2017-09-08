// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor {
    public static class FunctionDefinitionExtensions {
        public static IFunctionInfo MakeFunctionInfo(this IFunctionDefinition fd, string functionName, bool isInternal) {
            if (fd != null) {
                var fi = new FunctionInfo(functionName, isInternal) {
                    Signatures = new [] { fd.MakeSignature(functionName) }
                };
                return fi;
            }
            return null;
        }

        /// <summary>
        /// Constructs function signature based on function name and 
        /// the function definitions found in the AST.
        /// </summary>
        public static ISignatureInfo MakeSignature(this IFunctionDefinition fd, string functionName) {
            var si = new SignatureInfo(functionName) {Arguments = new List<IArgumentInfo>()};
            foreach (var arg in fd.Arguments) {
                if (arg is NamedArgument na) {
                    var defaultValue = na.DefaultValue != null ? fd.Root.TextProvider.GetText(na.DefaultValue) : string.Empty;
                    si.Arguments.Add(new ArgumentInfo(na.Name, string.Empty, defaultValue));
                } else {
                    if (arg is ExpressionArgument ea && ea.Children.Count > 0) {
                        if (ea.Children[0] is IExpression exp && exp.Children.Count > 0) {
                            if (exp.Children[0] is Variable v) {
                                si.Arguments.Add(new ArgumentInfo(v.Name));
                            }
                        }
                    }
                }
            }
            return si;
        }

        public static IFunctionDefinition FindFunctionDefinition(this AstRoot ast, int position, out IVariable v) {
            v = null;
            var exp = ast.GetNodeOfTypeFromPosition<IExpressionStatement>(position);
            return exp?.GetVariableOrFunctionDefinition(out v);
        }
    }
}
