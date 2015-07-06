using System.Diagnostics;
using System.Numerics;

namespace Microsoft.R.Core.AST.DataTypes
{
    /// <summary>
    /// Represents R complex number. Complex numbers are 
    /// scalars which are one element vectors of 'complex' mode.
    /// </summary>
    [DebuggerDisplay("[{Value}]")]
    public class RComplex: RScalar<Complex>
    {
        public override RMode Mode
        {
            get { return RMode.Complex;  }
        }
        public RComplex(Complex value): 
            base(value)
        {
        }
    }
}
