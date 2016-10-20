// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    /// <seealso cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/>
    /// <seealso cref="RValueInfo.Representation"/>
    public static class RValueRepresentations {
        /// <summary>
        /// Returns an R expression that evaluates to a function that is suitable for use as the <c>repr</c> argument
        /// when invoking <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/> and its wrappers. The representation
        /// produced by that function is the same as R function <c>toString()</c>, except that result is concatenated
        /// to form a single string.
        /// </summary>
        /// <remarks>
        /// Note that there are no facilities to limit the size of the output for this representation. Thus, it should
        /// be used very sparingly for unknown or untrusted inputs.
        /// </remarks>
        public static new string ToString => "rtvs:::repr_toString";

        /// <summary>
        /// Returns an R expression that evaluates to a function that is suitable for use as the <c>repr</c> argument
        /// when invoking <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/> and its wrappers. The representation
        /// produced by that function is the same as R function <c>deparse()</c>, except that result is concatenated
        /// to form a single string, and optionally truncated.
        /// </summary>
        /// <param name="maxLength">
        /// Maximum length of the output - if representation is longer than that, it is truncated to match. This value
        /// cannot be lower than 20, or higher than 500 - if it is, it is adjusted to fit into that range.
        /// If <see langword="null"/>, the represnetation is not truncated.
        /// </param>
        public static string Deparse(int? maxLength = null) =>
            Invariant($"rtvs:::make_repr_deparse({maxLength})");

        /// <summary>
        /// Returns an R expression that evaluates to a function that is suitable for use as the <c>repr</c> argument
        /// when invoking <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/> and its wrappers. The representation
        /// produced by that function is the same as R function <c>str()</c>, except that result is returned as a single
        /// string instead of being printed.
        /// </summary>
        /// <param name="maxLength">
        /// Maximum length of the output - if representation is longer than that, it is truncated to match, and 
        /// <paramref name="overflowSuffix"/> is appended. If <see langword="null"/>, the representation is not truncated.
        /// </param>
        /// <param name="expectedLength">
        /// An upper bound estimate of the expected length of the output. If the output fits into the specified length,
        /// it avoids potentially expensive reallocations of memory buffer used to capture the output of <c>str</c>,
        /// by preallocating the buffer in advance. This parameter is for optimization purposes only, and will not
        /// cause an error even if the output exceeds the expected length.  If <see langword="null"/>, a reasonable 
        /// default value is used for buffer size.
        /// </param>
        /// <param name="overflowSuffix">
        /// A string suffix that is appended to the truncated representation if it exceeded <paramref name="maxLength"/>.
        /// If <see langword="null"/>, <c>"..."</c> is used as a suffix.
        /// </param>
        /// <remarks>
        /// Single-element vectors are special-cased by removing the type prefix. For example, while <c>str(42)</c>
        /// will produce <c>"num 42"</c>, the function produced by this helper will return <c>"42"</c>.
        /// </remarks>
        public static string Str(int? maxLength = null, int? expectedLength = null, string overflowSuffix = null) =>
            Invariant($"rtvs:::make_repr_str({maxLength}, {expectedLength}, {overflowSuffix.ToRStringLiteral()})");
    }
}
