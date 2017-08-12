// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Statements.Loops;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST {
    public static class ScopeExtensions {
        /// <summary>
        /// Enumerates scopes from the current up to the global scope.
        /// </summary>
        public static IEnumerable<IScope> EnumerateTowardsGlobal(this IScope scope) {
            yield return scope;
            while (!(scope is GlobalScope)) {
                scope = scope.GetEnclosingScope();
                yield return scope;
            }
        }

        /// <summary>
        /// Enumerates definitions of variables applicable to the given scope.
        /// Traverses scopes from the provided scope up and enumerates
        /// variables that appear at the left side of the left-hand assignment
        /// operator or at the right side of the right-hand assignment operator.
        /// Includes both regular variables as well as function definitions.
        /// Includes assignments that appear in the file up to the specified
        /// position except in the global scope it enumerates all assignments.
        /// </summary>
        /// <param name="position"></param>
        public static IEnumerable<IVariable> GetApplicableVariables(this IScope scope, int position) {
            foreach (var s in scope.EnumerateTowardsGlobal()) {
                foreach (var v in s.GetScopeVariables(position)) {
                    yield return v;
                    position = s.Start;
                }
            }
        }

        /// <summary>
        /// Enumerates definitions of variables applicable to the given scope.
        /// Enumerates all the variables that appear at the left side of 
        /// the left assignment operator or at the right side of the right-hand 
        /// assignment operator. Includes both regular variables as well as 
        /// function definitions.
        /// </summary>
        /// <param name="scope">Scope to look into</param>
        /// <param name="position">
        /// If scope is not a global scope, then only variables before this position
        /// will be enumerated. In global scope all variables are enumerated. This
        /// reflects how variables are visible when file is sources and user types
        /// somewhere in inner scope.
        /// </param>
        public static IEnumerable<IVariable> GetScopeVariables(this IScope scope, int position) {
            bool globalScope = scope is GlobalScope;

            if (!globalScope) {
                // See if this is a function scope with arguments
                if (scope.Parent is IFunctionDefinition funcDef && funcDef.Arguments != null) {
                    foreach (var arg in funcDef.Arguments) {
                        if (arg is IVariable na) {
                            yield return na;
                        } else {
                            var ea = arg as ExpressionArgument;
                            if (ea?.ArgumentValue != null && ea.ArgumentValue.Children.Count == 1) {
                                if (ea.ArgumentValue.Children[0] is IVariable v) {
                                    yield return v;
                                }
                            }
                        }
                    }
                }

                var forStatement = scope.Parent as For;
                var enumExpression = forStatement?.EnumerableExpression;
                var variable = enumExpression?.Variable;
                if (variable != null) {
                    yield return variable;
                }
            }

            foreach (var c in scope.Children) {
                if (c is IExpressionStatement es) {
                    if (!globalScope && es.Start > position) {
                        // In local scope stop at the predefined location
                        // so we do not enumerate variables or functions
                        // that hasn't been declared yet in the scope flow.
                        yield break;
                    }

                    var fd = es.GetVariableOrFunctionDefinition(out IVariable v);
                    if (fd != null && v != null) {
                        v.Value = new RFunction(fd);
                    }
                    if (v != null) {
                        yield return v;
                    }
                }
            }
        }

        /// <summary>
        /// Locates function with a given name inside the scope. Only function
        /// definition that appear before the given position are analyzed except
        /// when scope is the global scope.
        /// </summary>
        public static IVariable FindFunctionDefinitionByName(this IScope scope, string name, int position) {
            var variables = scope.GetApplicableVariables(position);
            var v = variables.FirstOrDefault(x =>
                (x.Value is RFunction) && x.Name.EqualsOrdinal(name));
            return v;
        }

        /// <summary>
        /// Locates variable with a given name inside the scope. Only items
        /// that appear before the given position are analyzed except
        /// when scope is the global scope.
        /// </summary>
        public static IVariable FindVariableDefinitionByName(this IScope scope, string name, int position) {
            var variables = scope.GetApplicableVariables(position);
            return variables.FirstOrDefault(x => x.Name.EqualsOrdinal(name));
        }

        /// <summary>
        /// Attempts to retrieve value for the given KnitR block option.
        /// Works for scopes that define KnitR code chunk arguments
        /// in the R Markdown such as '{r echo=TRUE, warning=FALSE}
        /// </summary>
        public static string GetKnitrBlockOption(this IScope scope, string optionName) {
            if (scope == null || !scope.KnitrOptions) {
                return string.Empty;
            }

            // Find all expressions that look like 'name = value' 
            // and if name matches, return the value.
            var valueNode = scope.Children
                .OfType<IExpressionStatement>()
                .Select(s => s.Expression)
                .Where(e => e.Children.Count == 1 && (e.Children[0] as IOperator)?.OperatorType == OperatorType.Equals)
                .Select(e => (IOperator)e.Children[0])
                .Where(op => (op.LeftOperand as Variable)?.Name == optionName)
                .Select(op => op.RightOperand)
                .FirstOrDefault();

            return valueNode?.Root?.TextProvider?.GetText(valueNode) ?? string.Empty;
        }
    }
}
