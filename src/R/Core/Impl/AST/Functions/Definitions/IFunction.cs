using Microsoft.R.Core.AST.Arguments;

namespace Microsoft.R.Core.AST.Functions.Definitions
{
    public interface IFunction
    {
        TokenNode OpenBrace { get; }

        ArgumentList Arguments { get; }

        TokenNode CloseBrace { get; }

        int SignatureEnd { get; }
    }
}
