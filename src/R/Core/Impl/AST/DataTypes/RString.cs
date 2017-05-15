// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R string. String are scalars
    /// which are one element vectors of 'character' mode.
    /// </summary>
    public class RString : RScalar<string> {
        public override RMode Mode => RMode.Character;

        public RString(string value) :
            base(value) {
        }

        public static implicit operator RString(string s) => new RString(s);
        public static explicit operator string (RString rs) => rs.Value;
        public static bool operator ==(RString x, RString y) => x.Value == y.Value;
        public static bool operator !=(RString x, RString y) => x.Value != y.Value;


        public override bool Equals(object obj) {
            try {
                return Value == ((RString)obj).Value;
            } catch {
                return false;
            }
        }
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
    }
}
