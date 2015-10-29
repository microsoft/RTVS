using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Values {
    /// <summary>
    /// Represents NULL value
    /// </summary>
    public sealed class NullValue : RValueTokenNode<RNull> {
        public override bool Parse(ParseContext context, IAstNode parent) {
            NodeValue = new RNull();
            return base.Parse(context, parent);
        }
    }
}
