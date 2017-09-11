// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.R.Editor.Functions {
    public sealed class SignatureInfo : ISignatureInfo {
        public const int MaxSignatureLength = 160;

        public SignatureInfo(string functionName) {
            FunctionName = functionName;
        }

        #region ISignatureInfo
        /// <summary>
        /// Function name
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Function arguments
        /// </summary>
        public IList<IArgumentInfo> Arguments { get; set; }

        /// <summary>
        /// Creates formatted signature that is presented to the user
        /// during function parameter completion. Optionally provides
        /// locus points (locations withing the string) for each function
        /// parameter.
        /// </summary>
        public string GetSignatureString(string actualName, List<int> locusPoints = null) {
            var sb = new StringBuilder(actualName);
            var lineCount = 0;

            sb.Append('(');

            locusPoints?.Add(sb.Length);

            for (var i = 0; i < Arguments.Count; i++) {
                var arg = Arguments[i];
                sb.Append(arg.Name);

                if (!string.IsNullOrEmpty(arg.DefaultValue)) {
                    sb.Append(" = ");
                    sb.Append(arg.DefaultValue);
                }

                if (i < Arguments.Count - 1) {
                    sb.Append(", ");
                }

                if (locusPoints != null) {
                    locusPoints.Add(sb.Length);
                }

                if (sb.Length > (lineCount + 1) * MaxSignatureLength && i != Arguments.Count - 1) {
                    sb.Append("\r\n");
                    lineCount++;
                }
            }

            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Given argument name returns index of the argument in the signature.
        /// Performs full and then partial matching fof the argument name.
        /// </summary>
        /// <param name="argumentName">Name of the argument</param>
        /// <param name="partialMatch">
        /// If true, partial match will be performed 
        /// if exact match is not found
        /// </param>
        /// <returns>Argument index or -1 if argumen is not named or was not found</returns>
        public int GetArgumentIndex(string argumentName, bool partialMatch) {
            // A function f <- function(foo, bar) is said to have formal parameters "foo" and "bar", 
            // and the call f(foo=3, ba=13) is said to have (actual) arguments "foo" and "ba".
            // R first matches all arguments that have exactly the same name as a formal parameter. 
            // Two identical argument names cause an error. Then, R matches any argument names that
            // partially matches a(yet unmatched) formal parameter. But if two argument names partially 
            // match the same formal parameter, that also causes an error. Also, it only matches 
            // formal parameters before .... So formal parameters after ... must be specified using 
            // their full names. Then the unnamed arguments are matched in positional order to 
            // the remaining formal arguments.

            if (string.IsNullOrEmpty(argumentName)) {
                return -1;
            }

            // full name match first
            for (var i = 0; i < Arguments.Count; i++) {
                var argInfo = Arguments[i];
                if (argInfo.Name.Equals(argumentName, StringComparison.Ordinal)) {
                    return i;
                }
            }

            if (!partialMatch) {
                return -1;
            }

            // Try partial match. Only match unique or longest
            var minLengthDifference = Int32.MaxValue;
            var index = -1;
            var unique = true;

            for (var i = 0; i < Arguments.Count; i++) {
                var argInfo = Arguments[i];
                if (argInfo.Name.StartsWith(argumentName, StringComparison.Ordinal)) {
                    var lengthDifference = argInfo.Name.Length - argumentName.Length;
                    if (lengthDifference < minLengthDifference) {
                        minLengthDifference = lengthDifference;
                        index = i;
                        unique = true;
                    } else if (index >= 0) {
                        unique = false;
                    }
                }

                if (argInfo.IsEllipsis) {
                    break;
                }
            }

            return unique ? index : -1;
        }
        #endregion
    }
}
