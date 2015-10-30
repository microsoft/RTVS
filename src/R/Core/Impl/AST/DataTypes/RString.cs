using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R string. String are scalars
    /// which are one element vectors of 'character' mode.
    /// </summary>
    public class RString : RScalar<string> {
        public override RMode Mode {
            get { return RMode.Character; }
        }

        public RString(string value) :
            base(value) {
        }

        public static implicit operator RString(string s) {
            return new RString(s);
        }

        public static explicit operator string (RString rs) {
            return rs.Value;
        }

        public static bool operator ==(RString x, RString y) {
            return x.Value == y.Value;
        }

        public static bool operator !=(RString x, RString y) {
            return x.Value != y.Value;
        }


        public override bool Equals(object obj) {
            try {
                return this.Value == ((RString)obj).Value;
            } catch {
                return false;
            }
        }
        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }
    }
}
