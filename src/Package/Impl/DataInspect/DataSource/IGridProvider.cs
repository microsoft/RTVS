// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal interface IGridData<TData> {
        IRange<TData> ColumnHeader { get; }

        IRange<TData> RowHeader { get; }

        IGrid<TData> Grid { get; }
    }

    /// <summary>
    /// Two dimensional data provider
    /// </summary>
    /// <typeparam name="TData">data type</typeparam>
    internal interface IGridProvider<TData> {
        /// <summary>
        /// total number of items in row
        /// </summary>
        long RowCount { get; }

        /// <summary>
        /// total number of items in column
        /// </summary>
        long ColumnCount { get; }

        bool CanSort { get; }

        /// <summary>
        /// Fetches range or tabular data
        /// </summary>
        /// <param name="range">Range to fetch</param>
        /// <param name="sortOrder">Sequence of fields to sort by (in order)</param>
        /// <param name="decreasing">Ascending (increasing, default) or descending order</param>
        /// <returns></returns>
        Task<IGridData<TData>> GetAsync(GridRange range, ISortOrder sortOrder = null);
    }
}
