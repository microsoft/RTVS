// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST {
    public static class AstRootExtensions {

        /// <summary>
        /// Enumerates function definitions applicable to the current scope.
        /// Returns definitions that appear in the file up to the specified
        /// position.
        /// </summary>
        public static IEnumerable<RFunction> GetFunctionsFromPosition(this AstRoot ast, int position) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            var variables = scope.GetApplicableVariables(position);
            return variables.Where(x => x.Value is RFunction).Select(x => x.Value as RFunction);
        }

        /// <summary>
        /// Locates function or variable definition given the item name and 
        /// the position of the name in the text buffer.
        /// </summary>
        /// <returns>AST node that defines the specified item</returns>
        public static IAstNode FindItemDefinition(this AstRoot ast, int position, string itemName) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            var func = scope.FindFunctionDefinitionByName(itemName, position);
            return func ?? scope.FindVariableDefinitionByName(itemName, position);
        }

        public static bool IsCompleteExpression(this AstRoot expressionAst) {
            foreach (var error in expressionAst.Errors) {
                if (error.ErrorType == ParseErrorType.CloseCurlyBraceExpected ||
                    error.ErrorType == ParseErrorType.CloseBraceExpected ||
                    error.ErrorType == ParseErrorType.CloseSquareBracketExpected ||
                    error.ErrorType == ParseErrorType.FunctionBodyExpected ||
                    error.ErrorType == ParseErrorType.RightOperandExpected) {
                    return false;
                }
            }
            return true;
        }
    }
}
