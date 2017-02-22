// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Describes R object
    /// </summary>
    public interface IRObjectInformation {
        /// <summary>
        /// R object type name (in R terms)
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Object length (such an number of elements in a list)
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Number of dimensions in the object.
        /// </summary>
        IReadOnlyList<long> Dim { get; }
    }
}
