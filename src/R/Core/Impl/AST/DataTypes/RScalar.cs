// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents scalar (numerical or string) value. 
    /// Scalars are one-element vectors.
    /// </summary>
    [DebuggerDisplay("[{Value}]")]
    public abstract class RScalar<T> : RObject, IRVector, IRScalar<T> {
        #region IRVector
        public int Length {
            get { return 1; }
        }

        public abstract RMode Mode { get; }
        #endregion

        #region IRScalar
        public T Value { get; set; }
        #endregion

        protected RScalar(T value) {
            Value = value;
        }

        public override string ToString() => this.Value.ToString();
    }
}
