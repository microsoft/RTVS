// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    /// <summary>
    /// Represents sequence that consists of a keyword
    /// and has conditional or enumerable expression such as 
    /// while(...) or for(...) followed by { } scope.
    /// </summary>
    public interface IKeywordExpressionScope : IKeywordScopeStatement, IKeywordExpression {
    }
}
