using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes
{
    /// <summary>
    /// Represents R integer value. Integers are scalars
    /// which are one element vectors of 'numeric' mode.
    /// </summary>
    [DebuggerDisplay("[{Value}]")]
    public class RInteger: RScalar<int>
    {
        public override RMode Mode
        {
            get { return RMode.Numeric;  }
        }

        public RInteger(int value): 
            base(value)
        {
        }
    }
}
