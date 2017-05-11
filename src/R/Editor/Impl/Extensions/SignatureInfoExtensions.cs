// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Signatures {
    public static class SignatureInfoExtensions {
        /// <summary>
        /// Given position in the buffer finds index of the function parameter.
        /// </summary>
        public static int ComputeCurrentParameter(this ISignatureInfo signatureInfo, IEditorBufferSnapshot snapshot, AstRoot ast, int position, IREditorSettings settings) {
            var parameterInfo = ast.GetSignatureInfoFromBuffer(snapshot, position);
            var index = 0;

            if (parameterInfo != null) {
                index = parameterInfo.ParameterIndex;
                if (parameterInfo.NamedParameter) {
                    // A function f <- function(foo, bar) is said to have formal parameters "foo" and "bar", 
                    // and the call f(foo=3, ba=13) is said to have (actual) arguments "foo" and "ba".
                    // R first matches all arguments that have exactly the same name as a formal parameter. 
                    // Two identical argument names cause an error. Then, R matches any argument names that
                    // partially matches a(yet unmatched) formal parameter. But if two argument names partially 
                    // match the same formal parameter, that also causes an error. Also, it only matches 
                    // formal parameters before ... So formal parameters after ... must be specified using 
                    // their full names. Then the unnamed arguments are matched in positional order to 
                    // the remaining formal arguments.

                    var argumentIndexInSignature = signatureInfo.GetArgumentIndex(parameterInfo.ParameterName, settings.PartialArgumentNameMatch);
                    if (argumentIndexInSignature >= 0) {
                        index = argumentIndexInSignature;
                    }
                }
            }
            return index;
        }
    }
}
