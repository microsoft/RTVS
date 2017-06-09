// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// R vector. Most if not all R objects are vectors of some sort.
    /// In a vector all data must be of the same type. If types are
    /// different they usually should be coerced into strings.
    /// </summary>
    [DebuggerDisplay("[{Mode}, {Length}]")]
    public class RVector<T> : RObject, IRVector<T> {
        private Lazy<HybridDictionary> vector = new Lazy<HybridDictionary>(() => new HybridDictionary());

        public RVector(RMode mode, int length) {
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Mode = mode;
            Length = length;
        }

        #region IRVector<T>
        /// <summary>
        /// Vector mode (data type)
        /// </summary>
        public RMode Mode { get; private set; }

        /// <summary>
        /// Number of elements in the vector
        /// </summary>
        public int Length { get; }

        public virtual T this[int index] {
            get {
                if (vector.IsValueCreated && index >= 0 && index < vector.Value.Count) {
                    return (T)vector.Value[index];
                }

                return default(T);
            }
            set => vector.Value[index] = value;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<T> GetEnumerator() {
            if (vector.IsValueCreated) {
                return Collection<T>.Empty.GetEnumerator();
            }

            return Collection<T>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            if (vector.IsValueCreated) {
                return Collection<T>.Empty.GetEnumerator();
            }

            return Collection<T>.Empty.GetEnumerator();
        }
        #endregion
    }
}
