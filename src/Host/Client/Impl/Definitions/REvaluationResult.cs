// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Corresponds to R <c>ParseStatus</c> enum as used by <c>Rf_eval</c>.
    /// </summary>
    public enum RParseStatus {
        Null,
        OK,
        Incomplete,
        Error,
        EOF
    }

    public partial struct REvaluationResult {
        /// <summary>
        /// JSON result of evaluation.
        /// </summary>
        /// <remarks>
        /// Computed by serializing the immediate result of evaluation, as if by <c>rtvs:::toJSON</c>.
        /// Only valid when <see cref="REvaluationKind.RawResult"/> was not used; ; otherwise, <see langword="null"/>.
        /// </remarks>
        public JToken Result { get; }

        /// <summary>
        /// Raw result of evaluation.
        /// </summary>
        /// <remarks>
        /// Contains the result of <see cref="RHost.EvaluateAsync(string, REvaluationKind, CancellationToken)"/>
        /// if <see cref="REvaluationKind.RawResult"/> was used; otherwise, <see langword="null"/>.
        /// </remarks>
        public byte[] RawResult { get; }

        /// <summary>
        /// If evaluation failed because of an R runtime error, text of the error message.
        /// Otherwise, <see langword="null"/>.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Status code indicating the result of parsing the expression.</summary>
        /// <remarks>
        /// For a successful evaluation, this is always <see cref="RParseStatus.OK"/>.
        /// </remarks>
        public RParseStatus ParseStatus { get; }

        public REvaluationResult(string error, RParseStatus parseStatus) {
            Error = error;
            ParseStatus = parseStatus;
            Result = null;
            RawResult = null;
        }

        public REvaluationResult(JToken result, string error, RParseStatus parseStatus)
            : this(error, parseStatus) {
            Result = result;
        }

        public REvaluationResult(byte[] rawResult, string error, RParseStatus parseStatus)
            : this(error, parseStatus) {
            RawResult = rawResult;
        }

        public override string ToString() {
            var sb = new StringBuilder();

            if (RawResult != null) {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<raw ({0} bytes)>", RawResult.Length);
            } else if (Result != null) {
                sb.Append(Result);
            }

            if (ParseStatus != RParseStatus.OK) {
                sb.AppendFormat(CultureInfo.InvariantCulture, "; {0}", ParseStatus);
            }
            if (Error != null) {
                sb.AppendFormat(CultureInfo.InvariantCulture, "; '{0}'", Error);
            }
            return sb.ToString();
        }
    }
}

