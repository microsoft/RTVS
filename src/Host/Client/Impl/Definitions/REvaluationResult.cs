// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;
using System.Collections.Generic;

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

    public struct REvaluationResult {
        /// <summary>
        /// Result of evaluation.
        /// </summary>
        /// <remarks>
        /// Computed by serializing the immediate result of evaluation, as if by <c>rtvs:::toJSON</c>.
        /// </remarks>
        public JToken Result { get; }
        /// <summary>
        /// Result of evaluation for 'raw'
        /// </summary>
        /// <remarks>
        /// Contains the result of <see cref="RHost.EvaluateAsync(string, REvaluationKind, CancellationToken)"/>, 
        /// with <see cref="REvaluationKind.Raw"/>.
        /// </remarks>
        public List<byte[]> Raw { get; }

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
            Raw = null;
        }

        public REvaluationResult(JToken result, string error, RParseStatus parseStatus)
            : this(error, parseStatus) {
            Result = result;
        }

        public REvaluationResult(JToken result, string error, RParseStatus parseStatus, List<byte[]> raw)
            : this(error, parseStatus) {
            Result = result;
            Raw = raw;
        }

        public override string ToString() {
            var sb = new StringBuilder((Result ?? "").ToString());
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

