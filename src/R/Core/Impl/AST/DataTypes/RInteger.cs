// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Globalization;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R integer value. Integers are scalars
    /// which are one element vectors of 'numeric' mode.
    /// </summary>
    [DebuggerDisplay("[{Value}]")]
    public class RInteger : RScalar<int> {
        public override RMode Mode {
            get { return RMode.Numeric; }
        }

        public RInteger(int value) :
            base(value) {
        }

        public static implicit operator RInteger(int x) {
            return new RInteger(x);
        }

        public static explicit operator int (RInteger ri) {
            return ri.Value;
        }

        public static bool operator ==(RInteger x, RInteger y) {
            return x.Value == y.Value;
        }

        public static bool operator !=(RInteger x, RInteger y) {
            return x.Value != y.Value;
        }

        public static RInteger operator +(RInteger x, RInteger y) {
            return new RInteger(x.Value + y.Value);
        }

        public static RInteger operator -(RInteger x, RInteger y) {
            return new RInteger(x.Value - y.Value);
        }

        public static RInteger operator *(RInteger x, RInteger y) {
            return new RInteger(x.Value * y.Value);
        }

        public static RInteger operator /(RInteger x, RInteger y) {
            return new RInteger(x.Value / y.Value);
        }

        public override bool Equals(object obj) {
            try {
                return this.Value == ((RInteger)obj).Value;
            } catch {
                return false;
            }
        }

        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }
        public override string ToString() {
            return Value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
