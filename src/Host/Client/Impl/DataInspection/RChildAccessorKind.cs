// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.DataInspection {
    /// <seealso cref="IRValueInfo.AccessorKind"/>
    public enum RChildAccessorKind {
        /// <summary>
        /// Unknown or inapplicable - for example, when <see cref="IRValueInfo"/> is not a child of any parent.
        /// </summary>
        None,
        /// <summary>
        /// Operator <c>[[ ]]</c>.
        /// </summary>
        Brackets,
        /// <summary>
        /// Operator <c>$</c>.
        /// </summary>
        Dollar,
        /// <summary>
        /// Operator <c>@</c>.
        /// </summary>
        At,
    }
}
