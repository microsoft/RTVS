// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Expressions;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents sequence that consists of a keyword
    /// followed by braces and expression such as in
    /// statements like while(...) or for(...).
    /// </summary>
    public interface IKeywordExpression : IKeyword {
        TokenNode OpenBrace { get; }
        IExpression Expression { get; }
        TokenNode CloseBrace { get; }
    }
}
