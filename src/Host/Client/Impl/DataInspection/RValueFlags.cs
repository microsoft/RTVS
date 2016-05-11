// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.DataInspection {
    /// <seealso cref="IRValueInfo.Flags"/>.
    [Flags]
    public enum RValueFlags {
        None,
        /// <summary>
        /// Whether <c>is.atomic()</c> is true for this value.
        /// </summary>
        Atomic = 1 << 1,
        /// <summary>
        /// Whether <c>is.recursive()</c> is true for this value.
        /// </summary>
        Recursive = 1 << 2,
        /// <summary>
        /// Whether <c>has.parent()</c> is true for this value.
        /// </summary>
        HasParentEnvironment = 1 << 3,
    }
}
