using System;
using Microsoft.R.Core.AST.Operators;

namespace Microsoft.R.Editor.Signatures
{
    public sealed class ParametersInfo
    {
        public FunctionCall FunctionCall { get; private set; }

        public int ParameterIndex { get; private set; }

        public string FunctionName { get; private set; }

        public int SignatureEnd
        {
            get
            {
                return FunctionCall.CloseBrace != null ? FunctionCall.CloseBrace.End : FunctionCall.Arguments.End;
            }
        }

        public ParametersInfo(string functionName, FunctionCall functionCall, int parameterIndex)
        {
            if (functionName == null)
                throw new ArgumentNullException("functionName");

            if (functionCall == null)
                throw new ArgumentNullException("functionCall");

            FunctionName = functionName;
            FunctionCall = functionCall;
            ParameterIndex = parameterIndex;
        }
    }
}
