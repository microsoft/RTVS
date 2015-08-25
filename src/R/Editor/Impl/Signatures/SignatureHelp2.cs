using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Tree.Search;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures
{
    public partial class SignatureHelp
    {
        /// <summary>
        /// Given position in a text buffer finds method name.
        /// </summary>
        public static string GetFunctionNameFromBuffer(AstRoot astRoot, int position, out int signatureEnd)
        {
            FunctionCall functionCall;
            Variable functionVariable;

            signatureEnd = -1;

            if (GetFunction(astRoot, position, out functionCall, out functionVariable))
            {
                signatureEnd = functionCall.CloseBrace != null ? functionCall.CloseBrace.End : functionCall.Arguments.End;
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

            if (!GetFunction(astRoot, position, out functionCall, out functionVariable))
            {
                // Handle abc(,,, |
                if (position == snapshot.Length || char.IsWhiteSpace(snapshot[position]))
                {
                    for (int i = position - 1; i >= 0; i--)
                    {
                        char ch = snapshot[i];

                        if (!char.IsWhiteSpace(ch))
                        {
                            if (!GetFunction(astRoot, i, out functionCall, out functionVariable) || functionCall.CloseBrace != null)
                            {
                                return null;
                            }

                            position = (ch == ',' || ch == '(') ? i + 1 : i;
                            break;
                        }
                    }
                }
            }

            if(functionCall == null || functionVariable == null)
            {
                return null;
            }

            string functionName = functionVariable.Name;
            int signatureEnd = functionCall.CloseBrace != null ? functionCall.CloseBrace.End : functionCall.Arguments.End;
            int argCount = functionCall.Arguments.Count;

            if (argCount == 0)
            {
                parameterIndex = 0;
            }
            else
            {
                for (int i = 0; i < argCount; i++)
                {
                    IAstNode arg = functionCall.Arguments[i];
                    if (position < arg.End || (arg is MissingArgument && arg.Start == arg.End && arg.Start == 0))
                    {
                        parameterIndex = i;
                        break;
                    }
                }
            }

            if (parameterIndex < 0)
            {
                // func(... |  % comment
                if (functionCall.CloseBrace == null)
                {
                    ITextSnapshotLine textLine = snapshot.GetLineFromPosition(position);
                    TextRange range = TextRange.FromBounds(functionCall.OpenBrace.End - textLine.Start, position - textLine.Start);

                    string textBeforeCaret = textLine.GetText().Substring(range.Start, range.Length);
                    if (string.IsNullOrWhiteSpace(textBeforeCaret))
                    {
                        parameterIndex = functionCall.Arguments.Count;
                    }
                }
                else if (position <= functionCall.CloseBrace.Start)
                {
                    if (argCount > 0)
                    {
                        CommaSeparatedItem lastArgument = functionCall.Arguments[argCount - 1] as CommaSeparatedItem;
                        if (lastArgument.Comma != null && position >= lastArgument.Comma.End)
                        {
                            parameterIndex = argCount;
                        }
                        else
                        {
                            parameterIndex = argCount - 1;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(functionName) && functionCall != null && parameterIndex >= 0)
            {
                return new ParametersInfo(functionName, functionCall, parameterIndex);
            }

            return null;
        }

        private static bool GetFunction(AstRoot astRoot, int position, out FunctionCall functionCall, out Variable functionVariable)
        {
            functionVariable = null;

            functionCall = astRoot.GetNodeOfTypeFromPosition(position, (IAstNode node) =>
            {
                return node.GetType() == typeof(FunctionCall);
            }) as FunctionCall;

            if (functionCall != null && functionCall.Children.Count > 0)
            {
                functionVariable = functionCall.Children[0] as Variable;
                return functionVariable != null;
            }

            return false;
        }
    }
}
