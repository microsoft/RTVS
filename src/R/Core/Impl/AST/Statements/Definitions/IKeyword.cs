// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.Statements {
    /// <summary>
    /// Represents keyword sequence. Typically it appears 
    /// in a statement that is based on a keyword such as 
    /// 'while' or 'repeat'. Exception is inline 'if' which
    /// is an operand rather than a statement.
    /// </summary>
    public interface IKeyword {
        /// <summary>
        /// Statement keyword node
        /// </summary>
        TokenNode Keyword { get; }

        /// <summary>
        /// Keyword text
        /// </summary>
        string Text { get; }
    }
}
