using Microsoft.Languages.Editor.Completion;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Completion;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Signatures
{
    public partial class SignatureHelp
    {
        public static void TriggerSignatureHelp(ITextView textView) {
            CompletionController.DismissSignatureSession(textView);
            var rcc = RCompletionController.FromTextView(textView);
            rcc.TriggerSignatureHelp();
        }

        public static void DismissSession(ITextView textView, bool retrigger = false) {
            CompletionController.DismissSignatureSession(textView);
            if (retrigger) {
                var rcc = RCompletionController.FromTextView(textView);
                rcc.TriggerSignatureHelp();
            }
        }

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
        public static ParameterInfo GetParametersInfoFromBuffer(AstRoot astRoot, ITextSnapshot snapshot, int position)
        {
            FunctionCall functionCall;
            Variable functionVariable;
            int parameterIndex = -1;
            string parameterName;
            bool namedParameter = false;

            if (!GetFunction(astRoot, ref position, out functionCall, out functionVariable))
            {
                return null;
            }

            parameterIndex = functionCall.GetParameterIndex(position);
            parameterName = functionCall.GetParameterName(parameterIndex, out namedParameter);

            if (!string.IsNullOrEmpty(functionVariable.Name) && functionCall != null && parameterIndex >= 0)
            {
                return new ParameterInfo(functionVariable.Name, functionCall, parameterIndex, parameterName, namedParameter);
            }

            return null;
        }

        private static bool GetFunction(AstRoot astRoot, ref int position, out FunctionCall functionCall, out Variable functionVariable)
        {
            functionVariable = null;
            functionCall = astRoot.GetNodeOfTypeFromPosition<FunctionCall>(position);

            if (functionCall == null && position > 0)
            {
                // Retry in case caret is at the very end of function signature
                // that does not have final close brace yet.
                functionCall = astRoot.GetNodeOfTypeFromPosition<FunctionCall>(position - 1, includeEnd: true);
                if(functionCall != null)
                {
                    // But if signature does have closing brace and caret
                    // is beyond it, we are really otuside of the signature.
                    if(functionCall.CloseBrace != null && position >= functionCall.CloseBrace.End) {
                        return false;
                    }

                    if (position > functionCall.SignatureEnd) {
                        position = functionCall.SignatureEnd;
                    }
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
