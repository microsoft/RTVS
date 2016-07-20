// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents statement that is based on a keyword
    /// and has a scope such as 'repeat { }'.
    /// </summary>
    public interface IKeywordScopeStatement : IKeyword, IStatement, IAstNodeWithScope { }
}
