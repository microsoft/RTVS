// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Core.Tokens {
    public enum RTokenSubType {
        /// <summary>
        /// No subtype
        /// </summary>
        None,

        /// <summary>
        /// Function that has dots in the name or that is later 
        /// recognized by the parser as built-in.
        /// </summary>
        BuiltinFunction,

        /// <summary>
        /// Identifier that has dots in the name or that is later 
        /// recognized by the parser as built-in.
        /// </summary>
        BuiltinConstant,

        /// <summary>
        /// as.* and is.* functions
        /// </summary>
        TypeFunction,
    }
}
