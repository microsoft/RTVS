// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R logical. Logicals (booleans) are scalars
    /// which are one element vectors of 'logical' mode.
    /// </summary>
    [DebuggerDisplay("[{" + nameof(Value) + "}]")]
    public class RLogical : RScalar<bool> {
        public static RLogical FALSE = new RLogical(false);
        public static RLogical TRUE = new RLogical(true);

        public override RMode Mode => RMode.Logical;

        public RLogical(bool value) :
            base(value) {
        }

        public static implicit operator RLogical(bool x) => x ? TRUE : FALSE;
        public static implicit operator bool (RLogical x) => x.Value;

        public static bool operator ==(RLogical x, RLogical y) {
            return x.Value == y.Value;
        }

        public static bool operator !=(RLogical x, RLogical y) => x.Value != y.Value;
        public static RLogical operator !(RLogical x) => x.Value ? FALSE : TRUE;

        public static RLogical operator &(RLogical x, RLogical y) => x.Value & y.Value ? TRUE : FALSE;
        public static RLogical operator |(RLogical x, RLogical y) => x.Value | y.Value ? TRUE : FALSE;

        public static bool operator true(RLogical x) => x.Value;
        public static bool operator false(RLogical x) => x.Value == false;

        public override bool Equals(object obj) {
            try {
                return Value == ((RLogical)obj).Value;
            } catch {
                return false;
            }
        }
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value ? "TRUE" : "FALSE";
    }
}
