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
        /// Returns an R expression  function that takes a data frame or a matrix, and returns the result of
        /// invoking <c>order()</c> that corresponds to this sort order, applied to the argument. The returned
        /// string is suitable for passing as the <c>row_selector</c> argument of <c>rtvs:::grid_data</c>.
        /// </summary>
        string GetRowSelector();
    }
}
