// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST {
    public static class AstRootExtensions {

        /// <summary>
        /// Enumerates function definitions applicable to the current scope and position.
        /// </summary>
        public static IEnumerable<RFunction> GetFunctionsFromPosition(this AstRoot ast, int position) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            var variables = scope.GetApplicableVariables(position);
            return variables.Where(x => x.Value is RFunction).Select(x => x.Value as RFunction);
        }
    }
}
