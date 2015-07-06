using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Evaluation.Definitions
{
    /// <summary>
    /// Represents object that can evaluate R statements and expressions.
    /// </summary>
    public interface ICodeEvaluator
    {
        RObject Evaluate(IAstNode node);
    }
}
