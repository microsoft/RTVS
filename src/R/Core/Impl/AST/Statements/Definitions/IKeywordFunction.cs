// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Functions;

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents function which name is a keyword like switch()
    /// </summary>
    public interface IKeywordFunction : IKeyword, IFunction {
    }
}
