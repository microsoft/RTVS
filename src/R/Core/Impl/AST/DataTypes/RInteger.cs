// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Globalization;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R integer value. Integers are scalars
    /// which are one element vectors of 'numeric' mode.
    /// </summary>
    [DebuggerDisplay("[{" + nameof(Value) + "}]")]
    public class RInteger : RScalar<int> {
        public override RMode Mode => RMode.Numeric;

        public RInteger(int value) :
            base(value) {
        }

        public static implicit operator RInteger(int x) => new RInteger(x);
        public static explicit operator int (RInteger ri) => ri.Value;

        public static bool operator ==(RInteger x, RInteger y) => x.Value == y.Value;
        public static bool operator !=(RInteger x, RInteger y) => x.Value != y.Value;
        public static RInteger operator +(RInteger x, RInteger y) => new RInteger(x.Value + y.Value);
        public static RInteger operator -(RInteger x, RInteger y) => new RInteger(x.Value - y.Value);
        public static RInteger operator *(RInteger x, RInteger y) => new RInteger(x.Value * y.Value);
        public static RInteger operator /(RInteger x, RInteger y) => new RInteger(x.Value / y.Value);

        public override bool Equals(object obj) {
            try {
                return this.Value == ((RInteger)obj).Value;
            } catch {
                return false;
            }
        }

        public override int GetHashCode() => this.Value.GetHashCode();
        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);
    }
}
