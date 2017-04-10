// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Support.Help;

namespace Microsoft.R.Editor {
    public static class AstRootExtensions {
        public static IFunctionInfo GetUserFunctionInfo(this AstRoot ast, string functionName, int position) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            var v = scope?.FindFunctionDefinitionByName(functionName, position);
            var rf = v?.Value as RFunction;
            var fd = rf?.Value as IFunctionDefinition;
            return fd?.MakeFunctionInfo(functionName);
        }
    }
}
