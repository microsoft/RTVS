using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    [Flags]
    public enum REvaluationKind {
        Normal = 0,
        Reentrant = 1 << 1,
        Json = 1 << 2,
        BaseEnv = 1 << 3,
        EmptyEnv = 1 << 4,
        Cancelable = 1 << 5,
        UnprotectedEnv = 1 << 6,
    }

    public interface IRExpressionEvaluator {
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct);
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
