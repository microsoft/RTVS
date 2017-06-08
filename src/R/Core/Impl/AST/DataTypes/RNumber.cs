// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R numerical value. Numbers are scalars
    /// which are one element vectors of 'numeric' mode.
    /// </summary>
    [DebuggerDisplay("[{Value}]")]
    public class RNumber : RScalar<double> {
        public override RMode Mode => RMode.Numeric;

        public RNumber(double value) :
            base(value) {
        }

        public static implicit operator RNumber(double x) => new RNumber(x);
        public static explicit operator double (RNumber x) => x.Value;
        public static bool operator ==(RNumber x, RNumber y) => x.Value == y.Value;
        public static bool operator !=(RNumber x, RNumber y) => x.Value != y.Value;
        public static RNumber operator +(RNumber x, RNumber y) => new RNumber(x.Value + y.Value);
        public static RNumber operator -(RNumber x, RNumber y) => new RNumber(x.Value - y.Value);
        public static RNumber operator *(RNumber x, RNumber y) => new RNumber(x.Value * y.Value);

        public static RNumber operator /(RNumber x, RNumber y) {
            return new RNumber(x.Value / y.Value);
        }

        public override bool Equals(object obj) {
            try {
                return Value == ((RNumber)obj).Value;
            } catch {
                return false;
            }
        }
        public override int GetHashCode() => this.Value.GetHashCode();
    }
}
