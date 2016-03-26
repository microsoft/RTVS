// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Parser.Definitions;

namespace Microsoft.R.Core.AST.Scopes.Definitions {
    /// <summary>
    /// Represents a sequence of statements typically under control 
    /// of a parent statement such as for(...) { }. Statements may
    /// be enclosed in curly braces in which case scope can also 
    /// declare new local variables and functions.
    /// </summary>
    public interface IScope : IAstNode {
        string Name { get; }

        TokenNode OpenCurlyBrace { get; }

        TokenNode CloseCurlyBrace { get; }
    }
}
