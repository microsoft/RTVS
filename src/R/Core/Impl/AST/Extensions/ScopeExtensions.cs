// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST {
    public static class ScopeExtensions {
        /// <summary>
        /// Enumerates scopes from the current up to the global scope.
        /// Does not return current scope, enumeration starts with the
        /// parent (outer) scope.
        /// </summary>
        public static IEnumerable<IScope> EnumerateTowardsGlobal(this IScope scope) {
            if (scope is GlobalScope) {
                yield break;
            }
            yield return scope.GetScope();
        }

        /// <summary>
        /// Enumerates definitions of variables applicable to the given scope.
        /// Traverses scopes from the provided scope up and enumerates
        /// variables that appear at the left side of the left-hand assignment
        /// operator or at the right side of the right-hand assignmend operator.
        /// Includes both regular variables as well as function definitions.
        /// Includes assignments that appear in the file up to the specified
        /// position except in the global scope it enumerates all assignments.
        /// </summary>
        /// <param name="position"></param>
        public static IEnumerable<Variable> GetApplicableVariables(this IScope scope, int position) {
            foreach (var v in scope.GetScopeVariables(position)) {
                yield return v;
            }

            var innerScope = scope;
            foreach (var s in scope.EnumerateTowardsGlobal()) {
                foreach (var v in s.GetScopeVariables(innerScope.Start)) {
                    yield return v;
                }
                innerScope = s;
            }
        }

        /// <summary>
        /// Enumerates definitions of variables applicable to the given scope.
        /// Enumerates all the variables that appear at the left side of 
        /// the left assignment operator or at the right side of the right-hand 
        /// assignmend operator. Includes both regular variables as well as 
        /// function definitions.
        /// </summary>
        /// <param name="scope">Scope to look into</param>
        /// <param name="position">
        /// If scope is not a global scope, then only variables before this position
        /// will be enumerated. In global scope all variables are enumerated. This
        /// reflects how variables are visible when file is sources and user types
        /// somewhere in inner scope.
        /// </param>
        public static IEnumerable<Variable> GetScopeVariables(this IScope scope, int position) {
            bool globalScope = scope is GlobalScope;
            foreach (var c in scope.Children) {
                var es = c as IExpressionStatement;
                if (es != null) {
                    if (!globalScope && es.Start >= position) {
                        yield break;
                    }

                    Variable v;
                    var fd = es.GetVariableOrFunctionDefinition(out v);
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
        /// when scope is a global scope.
        /// </summary>
        public static RFunction FindFunctionByName(this IScope scope, string name, int position) {
            var variables = scope.GetApplicableVariables(position);
            var v = variables.FirstOrDefault(x =>
                x.Name.EqualsOrdinal(name) && (x.Value is RFunction));
            return v?.Value as RFunction;
        }
    }
}
