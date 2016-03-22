// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST {
    public static class ScopeExtensions {
        /// <summary>
        /// Enumerates scopes from the current up to the global scope
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
        /// variables that appear at the left side of the left assignment
        /// operator or at the right side of the right-hand assignmend operator.
        /// Includes both regular variables as well as function definitions.
        /// </summary>
        /// <remarks>
        /// Limitations: current code does not analyze position of the scope inside
        /// outer scopes so variables declared in the outer scope after the current
        /// scope are not filtered and will appers in the completion list.
        /// </remarks>
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
        /// <param name="position">Only variables before this position will be enumerated</param>
        public static IEnumerable<Variable> GetScopeVariables(this IScope scope, int position) {
            foreach (IExpressionStatement es in scope.Children) {
                if (es.Start >= position) {
                    yield break;
                }

                Variable v;
                var fd = es.GetFunctionDefinition(out v);
                if (fd != null) {
                    v.Value = new RFunction(fd);
                }
                yield return v;
            }
        }

        /// <summary>
        /// Locates variable with a given name inside the scope. Only variable
        /// declarations that appear before the given position are analyzed.
        /// </summary>
        /// <returns></returns>
        public static RFunction FindFunctionByName(this IScope scope, string name, int position) {
            var v = scope.GetApplicableVariables(position).FirstOrDefault(x =>
                x.Name.Equals(name, StringComparison.Ordinal) && x.Value is RFunction);
            return v?.Value as RFunction;
        }
    }
}
