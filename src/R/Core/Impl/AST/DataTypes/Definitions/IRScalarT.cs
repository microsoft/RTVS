// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.DataTypes.Definitions {
    /// <summary>
    /// Represents scalar (numerical, string, boolean) value. 
    /// Scalars are one-element vectors.
    /// </summary>
    public interface IRScalar<T> {
        T Value { get; set; }
    }
}
