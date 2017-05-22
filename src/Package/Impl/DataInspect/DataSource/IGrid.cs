// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Abstract two dimentional data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGrid<T> {
        GridRange Range { get; }

        /// <summary>
        /// Return value at given position
        /// </summary>
        /// <param name="rowIndex">row index, zero based</param>
        /// <param name="columnIndex">column index, zero based</param>
        /// <returns>item value</returns>
        /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
        /// <exception cref="InvalidOperationException">when failed at setting or getting the value</exception>
        /// <exception cref="NotSupportedException">setter, when the grid is read only</exception>
        T this[long rowIndex, long columnIndex] {
            get; set;
        }
    }

    public interface IRange<T> {
        Range Range { get; }

        T this[long index] {
            get; set;
        }
    }
}
