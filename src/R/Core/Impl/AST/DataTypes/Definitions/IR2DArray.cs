// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.DataTypes.Definitions {
    /// <summary>
    /// Represents 2D array.
    /// </summary>
    public interface IR2DArray {
        /// <summary>
        /// Number of rows
        /// </summary>
        int NRow { get; }

        /// <summary>
        /// Number of columns
        /// </summary>
        int NCol { get; }

        /// <summary>
        /// Row names, if any
        /// </summary>
        RString[] RowNames { get; set; }

        /// <summary>
        /// Column names, if any
        /// </summary>
        RString[] ColNames { get; set; }
    }
}
