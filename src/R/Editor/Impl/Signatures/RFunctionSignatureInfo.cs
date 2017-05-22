// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Operators;

namespace Microsoft.R.Editor.Signatures {
    public sealed class RFunctionSignatureInfo {
        public string PackageName { get; }
        public string FunctionName { get; }
        public FunctionCall FunctionCall { get; }
        public int ParameterIndex { get; }
        public string ParameterName { get; }
        public bool NamedParameter { get; }

        public RFunctionSignatureInfo(string packageName, string functionName, FunctionCall functionCall, int parameterIndex, string parameterName, bool namedParameter) {
            PackageName = packageName;
            FunctionName = functionName;
            FunctionCall = functionCall;
            ParameterIndex = parameterIndex;
            ParameterName = parameterName;
            NamedParameter = namedParameter;
        }
    }
}
