// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Core.AST {
    /// <summary>
    /// Describes type of text position within AST
    /// </summary>
    public enum PositionType {
        /// <summary>
        /// Undefined position
        /// </summary>
        Undefined,

        /// <summary>
        /// Position is within node but type 
        /// of the node is unclear
        /// </summary>
        Node,

        /// <summary>
        /// Position is inside a comment or a string
        /// </summary>
        Token,
    }
}
