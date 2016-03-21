// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST {
    public static class AstNodeExtensions {
        /// <summary>
        /// Locates enclosing scope for a given node
        /// </summary>
        public static IScope GetScope(this IAstNode node) {
            var n = node.Parent;
            while (!(n is IScope)) {
                node = node.Parent;
             }
            return n as IScope;
        }

        /// <summary>
        /// Enumerates scopes from current up to the global scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IEnumerable<IScope> EnumerateTowardsGlobal(this IScope scope) {
            if(scope is GlobalScope) {
                yield break;
            }
            yield return scope.GetScope();
        }

        public static IEnumerable<Variable> GetApplicableVariables(this IScope scope) {
            foreach(var v in scope.GetScopeVariables()) {
                yield return v;
            }
            foreach (var s in scope.EnumerateTowardsGlobal()) {
                foreach (var v in s.GetScopeVariables()) {
                    yield return v;
                }
            }
        }

        public static IEnumerable<Variable> GetScopeVariables(this IScope scope) {
            foreach (IExpressionStatement es in scope.Children) {
                var c = es.Expression.Children;
                if (c.Count == 1) {
                    var op = c[0] as IOperator;
                    if (op != null) {
                        Variable v = null;
                        if (op.OperatorType == OperatorType.LeftAssign) {
                            v = op.LeftOperand as Variable;
                        } else if (op.OperatorType == OperatorType.RightAssign) {
                            v = op.LeftOperand as Variable;
                        }
                        if (v != null) {
                            var fd = op.GetFunctionDefinition();
                            if (fd != null) {
                                v.Value = new RFunction(fd);
                            }
                            yield return v;
                        }
                    }
                }
            }
        }

        public static IFunctionDefinition GetFunctionDefinition(this IOperator op) {
            var exp = op.RightOperand as IExpression;
            if (exp != null && exp.Children.Count == 1) {
                return exp.Children[0] as IFunctionDefinition;
            }
            return null;
        }
    }
}
