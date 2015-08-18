using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree.Search;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures
{
    public partial class SignatureHelp
    {
        /// <summary>
        /// Given position in a text buffer finds method name, 
        /// parameter index as well as where method signature ends.
        /// </summary>
        public static bool GetParameterPositionsFromBuffer(EditorDocument editorDocument, int position, 
                               out string functionName, out int parameterIndex, out int signatureEnd)
        {
            functionName = null;
            parameterIndex = 0;
            signatureEnd = -1;

            ITextSnapshot treeSnapshot = editorDocument.EditorTree.TextSnapshot;
            AstRoot ast = editorDocument.EditorTree.AstRoot;

            FunctionCall functionCall = ast.GetNodeOfTypeFromPosition(position, (IAstNode node) =>
                {
                    return node.GetType() == typeof(FunctionCall);
                }) as FunctionCall;

            if(functionCall == null)
            {
                return false;
            }

            // Let's see if this is actually a function call of a call 
            // operator applied to a return value or to an indexed argument 
            // such as x[2](param) or a()(b)(param). In the latter case, 
            // we'd need code evaluation to figure out the return type
            // and the the function signature. Currently it is not supported.
            // In the regular function call the preceding item in the 
            // expression is a variable.

            IAstNode parent = functionCall.Parent;
            Variable functionVariable = null;

            if (functionCall.Children.Count> 0)
            {
                functionVariable = functionCall.Children[0] as Variable;
                if (functionVariable != null)
                {
                    functionName = functionVariable.Name;
                    signatureEnd = functionCall.CloseBrace != null ? functionCall.CloseBrace.End : functionCall.Arguments.End;
                }

                for (int i = 0; i < functionCall.Arguments.Count; i++)
                {
                    IAstNode arg = functionCall.Arguments[i];

                    if (arg.Contains(position))
                    {
                        parameterIndex = i;
                        break;
                    }

                    if (arg.End <= position)
                    {
                        break;
                    }
                }
            }

            return !string.IsNullOrEmpty(functionName);
        }
    }
}
