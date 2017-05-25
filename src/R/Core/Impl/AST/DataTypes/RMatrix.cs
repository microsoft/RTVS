// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// R matrix. Matrix is a 2D vector.
    /// </summary>
    [DebuggerDisplay("[{Mode}, {NRow} x {NCol}]")]
    public sealed class RMatrix<T> : RArray<RArray<T>>, IR2DArray {
        public RMatrix(RMode mode, int nrow, int ncol) :
            base(mode, nrow * ncol) {
            this.NRow = nrow;
            this.NCol = ncol;
        }

        #region IR2DArray
        /// <summary>
        /// Number of rows
        /// </summary>
        public int NRow { get; private set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        public int NCol { get; private set; }

        /// <summary>
        /// Row names, if any
        /// </summary>
        public RString[] RowNames {
            get {
                RString[] names = new RString[this.NRow];
                int i = 0;
                foreach (RArray<T> array in this) {
                    names[i++] = array.DimName;
                }

                return names;
            }
            set {
                int i = 0;
                foreach (RString name in value) {
                    this[i++].DimName = name;

                    if (i >= this.NRow) {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Column names, if any
        /// </summary>
        public RString[] ColNames { get; set; }
        #endregion
    }
}
