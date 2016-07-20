// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST;

namespace Microsoft.R.Core.Parser {
    /// <summary>
    /// Represents an item that can be parsed. Used in recursive
    /// R language parser to construct syntax tree. All items
    /// in the AST implement this interface.
    /// </summary>
    public interface IParseItem {
        /// <summary>
        /// Parses the item.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if parsing is successful, false otherwise</returns>
        bool Parse(ParseContext context, IAstNode parent = null);
    }
}
