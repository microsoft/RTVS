// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Core.AST.Operators;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Describes parameter (actual argument) in a function call
    /// </summary>
    public sealed class ParameterInfo {
        /// <summary>
        /// Function call
        /// </summary>
        public FunctionCall FunctionCall { get; }

        /// <summary>
        /// Parameter index in the function call arguments
        /// </summary>
        public int ParameterIndex { get; }

        /// <summary>
        /// Parameter name if parameter is a named parameter
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// If true then the parameter is a named parameter in a function call
        /// </summary>
        public bool NamedParameter { get; }

        /// <summary>
        /// Function name
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Package name
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// Function signature end in the current text snapshot
        /// </summary>
        public int SignatureEnd => FunctionCall.SignatureEnd;

        public ParameterInfo(string packageName, string functionName, FunctionCall functionCall, int parameterIndex, string parameterName, bool namedParameter) {
            Check.ArgumentNull(nameof(functionName), functionName);
            Check.ArgumentNull(nameof(functionCall), functionCall);

            PackageName = packageName;
            FunctionName = functionName;
            FunctionCall = functionCall;
            ParameterIndex = parameterIndex;
            ParameterName = parameterName;
            NamedParameter = namedParameter;
        }
    }
}
