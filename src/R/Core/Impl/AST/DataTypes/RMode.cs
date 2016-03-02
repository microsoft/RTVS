// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// R 'mode' which is a data type. For example, string
    /// is a one-element vector of 'character' mode. Number
    /// is a single element vector of 'numerical' mode.
    /// Note that this enumeration does not list all possible
    /// runtime type, only those used by IDE and evluation engine.
    /// </summary>
    public enum RMode {
        Null,

        /// <summary>
        /// String
        /// </summary>
        Character,

        /// <summary>
        /// Integer or double
        /// </summary>
        Numeric,

        /// <summary>
        /// Boolean
        /// </summary>
        Logical,

        /// <summary>
        /// Complex number
        /// </summary>
        Complex,

        /// <summary>
        /// General list type
        /// </summary>
        List,

        /// <summary>
        /// Function type object
        /// </summary>
        Function
    }
}
