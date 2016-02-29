// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.DataTypes.Definitions {
    public interface IRArray<T> : IRVector<T> {
        /// <summary>
        /// Dimension name. Mostly used in multi-dimensional cases.
        /// </summary>
        RString DimName { get; set; }
    }
}
