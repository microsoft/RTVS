// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R logical. Logicals (booleans) are scalars
    /// which are one element vectors of 'logical' mode.
    /// </summary>
    [DebuggerDisplay("[{Value}]")]
    public class RLogical : RScalar<bool> {
        public static RLogical FALSE = new RLogical(false);
        public static RLogical TRUE = new RLogical(true);

        public override RMode Mode {
            get { return RMode.Logical; }
        }

        public RLogical(bool value) :
            base(value) {
        }

        public static implicit operator RLogical(bool x) {
            return x ? RLogical.TRUE : RLogical.FALSE;
        }

        public static implicit operator bool (RLogical x) {
            return x.Value;
        }

        public static bool operator ==(RLogical x, RLogical y) {
            return x.Value == y.Value;
        }

        public static bool operator !=(RLogical x, RLogical y) {
            return x.Value != y.Value;
        }

        public static RLogical operator !(RLogical x) {
            return x.Value ? RLogical.FALSE : RLogical.TRUE;
        }

        public static RLogical operator &(RLogical x, RLogical y) {
            return (x.Value & y.Value) ? RLogical.TRUE : RLogical.FALSE;
        }

        public static RLogical operator |(RLogical x, RLogical y) {
            return (x.Value | y.Value) ? RLogical.TRUE : RLogical.FALSE;
        }

        public static bool operator true(RLogical x) {
            return x.Value == true;
        }

        public static bool operator false(RLogical x) {
            return x.Value == false;
        }

        public override bool Equals(object obj) {
            try {
                return this.Value == ((RLogical)obj).Value;
            } catch {
                return false;
            }
        }
        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }
    }
}
