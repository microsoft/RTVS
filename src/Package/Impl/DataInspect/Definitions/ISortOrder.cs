// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Describes sorting set in details grid
    /// </summary>
    internal interface ISortOrder {
        /// <summary>
        /// No sorting is specified
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns sort direction of the primary column
        /// </summary>
        bool IsPrimaryDescending { get; }

        /// <summary>
        /// Returns expression to evaluate in R when ordering the data frame
        /// </summary>
        string GetDataFrameSortFunction();
    }
}
