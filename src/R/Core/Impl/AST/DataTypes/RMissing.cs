using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes
{
    public sealed class RMissing : RObject, IRVector
    {
        public static RMissing NA = new RMissing();

        public int Length
        {
            get { return 0; }
        }

        public RMode Mode
        {
            get { return RMode.Logical; }
        }
    }
}
