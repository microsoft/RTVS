// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Expressions;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Statement that is based on expression. Expression 
    /// can be mathematical, conditional, assignment, function 
    /// or operator definition.
    /// </summary>
    public interface IExpressionStatement : IStatement {
        IExpression Expression { get; }
    }
}
