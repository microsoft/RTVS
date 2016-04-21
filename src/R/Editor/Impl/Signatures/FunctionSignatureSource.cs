// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Editor.Signatures {
    public static class FunctionSignatureSource {
        public static Task<IFunctionSignature> GetSignatureAsync(string functionName) {
            // TODO: handle user-defined function
            // TODO: handle stack frames in debug mode
            //IFunctionInfo functionInfo = ast.GetUserFunctionInfo(functionName, 0);
            //if (functionInfo != null) {
            //    return Task.FromResult(FromFunctionInfo(functionInfo));
            //}

            var tcs = new TaskCompletionSource<IFunctionSignature>();
            // Then try package functions
            IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo(functionName, (o) => {
                functionInfo = FunctionIndex.GetFunctionInfo(functionName);
                tcs.TrySetResult(FromFunctionInfo(functionInfo));
            });

            if (functionInfo != null) {
                tcs.TrySetResult(FromFunctionInfo(functionInfo));
            }

            return tcs.Task;
        }

        private static IFunctionSignature FromFunctionInfo(IFunctionInfo functionInfo) {
            var signatureInfo = functionInfo?.Signatures?.FirstOrDefault();
            // Single signature for now
            string signatureString = signatureInfo?.GetSignatureString();
            if (string.IsNullOrEmpty(signatureString)) {
                string documentation = functionInfo?.Description?.Wrap(Math.Min(SignatureInfo.MaxSignatureLength, signatureString.Length));
                return new FunctionSignature(signatureString, documentation);
            }
            return null;
        }
    }
}
