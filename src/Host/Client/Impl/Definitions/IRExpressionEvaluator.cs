// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    [Flags]
    public enum REvaluationKind {
        Normal = 0,
        Reentrant = 1 << 1,
        Json = 1 << 2,
        BaseEnv = 1 << 3,
        EmptyEnv = 1 << 4,
        Cancelable = 1 << 5,
        NewEnv = 1 << 6,
        Mutating = 1 << 7,
    }

    public interface IRExpressionEvaluator {
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken));
    }

    public static class RExpressionEvaluatorExtensions {
        public static Task<REvaluationResult> EvaluateAsync(this IRExpressionEvaluator evaluator, FormattableString expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) {
            return evaluator.EvaluateAsync(Invariant(expression), kind, cancellationToken);
        }
    }

    public enum RParseStatus {
        Null,
        OK,
        Incomplete,
        Error,
        EOF
    }

    public struct REvaluationResult {
        public string StringResult { get; }
        public JToken JsonResult { get; }
        public string Error { get; }
        public RParseStatus ParseStatus { get; }

        public REvaluationResult(string result, string error, RParseStatus parseStatus) {
            StringResult = result;
            JsonResult = null;
            Error = error;
            ParseStatus = parseStatus;
        }

        public REvaluationResult(JToken result, string error, RParseStatus parseStatus) {
            StringResult = null;
            JsonResult = result;
            Error = error;
            ParseStatus = parseStatus;
        }

        public override string ToString() {
            var sb = new StringBuilder((StringResult ?? JsonResult ?? "").ToString());
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
