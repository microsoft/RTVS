using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes
{
    /// <summary>
    /// Represents R string. String are scalars
    /// which are one element vectors of 'character' mode.
    /// </summary>
    public class RString : RScalar<string>
    {
        public override RMode Mode
        {
            get { return RMode.Character; }
        }
        public RString(string value) :
            base(value)
        {
        }
    }
}
