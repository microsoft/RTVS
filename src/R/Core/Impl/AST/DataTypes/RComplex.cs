// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Numerics;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R complex number. Complex numbers are 
    /// scalars which are one element vectors of 'complex' mode.
    /// </summary>
    [DebuggerDisplay("[{" + nameof(Value) + "}]")]
    public class RComplex : RScalar<Complex> {
        public override RMode Mode => RMode.Complex;

        public RComplex(Complex value) :
            base(value) {
        }
    }
}
