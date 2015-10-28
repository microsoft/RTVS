using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    public sealed class RNull : RObject, IRVector {
        public static readonly RNull Null = new RNull();

        public int Length {
            get { return 0; }
        }

        public RMode Mode {
            get { return RMode.Null; }
        }
    }
}
