// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Scroll orientation of Grid
    /// </summary>
    [Flags]
    internal enum ScrollDirection {
        /// <summary>
        /// grid doesn't scroll
        /// </summary>
        None = 0x00,

        /// <summary>
        /// grid scrolls horizontally
        /// </summary>
        Horizontal = 0x01,

        /// <summary>
        /// grid scrolls vertically
        /// </summary>
        Vertical = 0x02,

        /// <summary>
        /// grid scrolls in vertical and horizontal direction
        /// </summary>
        Both = 0x03,
    }
}
