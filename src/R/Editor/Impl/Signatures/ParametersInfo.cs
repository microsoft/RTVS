using System;
using Microsoft.R.Core.AST.Operators;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Describes parameter (actual argument) in a function call
    /// </summary>
    public sealed class ParameterInfo {
        /// <summary>
        /// Function call
        /// </summary>
        public FunctionCall FunctionCall { get; private set; }

        /// <summary>
        /// Parameter index in the function call arguments
        /// </summary>
        public int ParameterIndex { get; private set; }

        /// <summary>
        /// Parameter name if parameter is a named parameter
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// If true then the parameter is a named parameter in a function call
        /// </summary>
        public bool NamedParameter { get; private set; }

        /// <summary>
        /// Function name
        /// </summary>
        public string FunctionName { get; private set; }

        /// <summary>
        /// Function signature end in the current text snapshot
        /// </summary>
        public int SignatureEnd {
            get { return FunctionCall.SignatureEnd; }
        }

        public ParameterInfo(string functionName, FunctionCall functionCall, int parameterIndex, string parameterName, bool namedParameter) {
            if (functionName == null)
                throw new ArgumentNullException("functionName");

            if (functionCall == null)
                throw new ArgumentNullException("functionCall");

            FunctionName = functionName;
            FunctionCall = functionCall;
            ParameterIndex = parameterIndex;
            ParameterName = parameterName;
            NamedParameter = namedParameter;
        }
    }
}
