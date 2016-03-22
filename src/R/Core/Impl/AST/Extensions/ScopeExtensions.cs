// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST {
    public static class ScopeExtensions {
        /// <summary>
        /// Enumerates scopes from current up to the global scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IEnumerable<IScope> EnumerateTowardsGlobal(this IScope scope) {
            if (scope is GlobalScope) {
                yield break;
            }
            yield return scope.GetScope();
        }

        public static IEnumerable<Variable> GetApplicableVariables(this IScope scope) {
            foreach (var v in scope.GetScopeVariables()) {
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

        public static RFunction FindFunctionByName(this IScope scope, string name) {
            var v = scope.GetApplicableVariables().FirstOrDefault(x =>
                x.Name.Equals(name, StringComparison.Ordinal) && x.Value is RFunction);
            return v?.Value as RFunction;
        }
    }
}
