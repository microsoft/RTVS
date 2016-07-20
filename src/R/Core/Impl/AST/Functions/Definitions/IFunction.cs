// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Arguments;

namespace Microsoft.R.Core.AST.Functions {
    public interface IFunction: IAstNode {
        /// <summary>
        /// Opening brace. Always present.
        /// </summary>
        TokenNode OpenBrace { get; }

        /// <summary>
        /// Function arguments
        /// </summary>
        ArgumentList Arguments { get; }

        /// <summary>
        /// Closing brace. May be null if closing brace is missing.
        /// </summary>
        TokenNode CloseBrace { get; }

        /// <summary>
        /// Returns end of a function signature (list of arguments).
        /// In case closing brace is missing scope extends to a
        /// nearest recovery point which may be an identifier
        /// or a keyword (except inline 'if').
        /// </summary>
        int SignatureEnd { get; }
    }
}
