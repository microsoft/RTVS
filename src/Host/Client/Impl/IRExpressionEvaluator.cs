using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRExpressionEvaluator
    {
        Task<REvaluationResult> EvaluateAsync(string expression);
    }

    public enum RParseStatus
    {
        Null,
        OK,
        Incomplete,
        Error,
        EOF
    }

    public struct REvaluationResult
    {
        public string Result { get; }
        public string Error { get; }
        public RParseStatus ParseStatus { get; }

        public REvaluationResult(string result, string error, RParseStatus parseStatus) {
            Result = result;
            Error = error;
            ParseStatus = parseStatus;
        }

        public override string ToString() {
            var sb = new StringBuilder(Result);
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
