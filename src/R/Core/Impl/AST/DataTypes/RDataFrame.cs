// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Data frame is a matrix where elements in each column 
    /// all have the same mode (data type). Data frame is typically
    /// constructed by reading data set from a database.
    /// </summary>
    [DebuggerDisplay("[{Length}]")]
    public sealed class RDataFrame : RList, IR2DArray {
        #region IR2DArray
        /// <summary>
        /// Number of rows
        /// </summary>
        public int NRow { get; private set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        public int NCol { get; private set; }
        #endregion

        public RString[] RowNames { get; set; }

        /// <summary>
        /// Column names, if any
        /// </summary>
        public RString[] ColNames { get; set; }

        public RMode[] ColModes { get; private set; }

        public RDataFrame(RMode[] columnModes, int nrow, int ncol) : base() {
            this.ColModes = columnModes;
            this.NRow = nrow;
            this.NCol = ncol;
        }
    }
}
