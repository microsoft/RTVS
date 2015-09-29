using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures
{
    public partial class SignatureHelp
    {
        /// <summary>
        /// Given position in a text buffer finds method name.
        /// </summary>
        public static string GetFunctionNameFromBuffer(AstRoot astRoot, ref int position, out int signatureEnd)
        {
            FunctionCall functionCall;
            Variable functionVariable;

            signatureEnd = -1;

            if (GetFunction(astRoot, ref position, out functionCall, out functionVariable))
            {
                signatureEnd = functionCall.End;
                return functionVariable.Name;
            }

            return null;
        }

        /// <summary>
        /// Given position in a text buffer finds method name, 
        /// parameter index as well as where method signature ends.
        /// </summary>
        public static ParametersInfo GetParametersInfoFromBuffer(AstRoot astRoot, ITextSnapshot snapshot, int position)
        {
            FunctionCall functionCall;
            Variable functionVariable;
            int parameterIndex = -1;

            if (!GetFunction(astRoot, ref position, out functionCall, out functionVariable))
            {
                return null;
            }

            parameterIndex = functionCall.GetParameterIndex(position);

            if (!string.IsNullOrEmpty(functionVariable.Name) && functionCall != null && parameterIndex >= 0)
            {
                return new ParametersInfo(functionVariable.Name, functionCall, parameterIndex);
            }

            return null;
        }

        private static bool GetFunction(AstRoot astRoot, ref int position, out FunctionCall functionCall, out Variable functionVariable)
        {
            functionVariable = null;
            functionCall = astRoot.GetNodeOfTypeFromPosition<FunctionCall>(position);

            if (functionCall == null && position > 0)
            {
                functionCall = astRoot.GetNodeOfTypeFromPosition<FunctionCall>(position - 1, includeEnd: true);
                if(functionCall != null && position > functionCall.SignatureEnd)
                {
                    position = functionCall.SignatureEnd;
                }
            }

            if (functionCall != null && functionCall.Children.Count > 0)
            {
                functionVariable = functionCall.Children[0] as Variable;
                return functionVariable != null;
            }

            return false;
        }
    }
}
