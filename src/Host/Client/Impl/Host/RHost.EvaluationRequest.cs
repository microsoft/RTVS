using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class EvaluationRequest {
            public readonly string Id;
            public readonly string MessageName;
            public readonly string Expression;
            public readonly REvaluationKind Kind;
            public readonly TaskCompletionSource<REvaluationResult> CompletionSource = new TaskCompletionSource<REvaluationResult>();

            public EvaluationRequest(RHost host, string expression, REvaluationKind kind, out JArray message) {
                Expression = expression;
                Kind = kind;

                var nameBuilder = new StringBuilder("=");
                if (kind.HasFlag(REvaluationKind.Reentrant)) {
                    nameBuilder.Append('@');
                }
                if (kind.HasFlag(REvaluationKind.Cancelable)) {
                    nameBuilder.Append('/');
                }
                if (kind.HasFlag(REvaluationKind.Json)) {
                    nameBuilder.Append('j');
                }
                if (kind.HasFlag(REvaluationKind.BaseEnv)) {
                    nameBuilder.Append('B');
                }
                if (kind.HasFlag(REvaluationKind.EmptyEnv)) {
                    nameBuilder.Append('E');
                }
                if (kind.HasFlag(REvaluationKind.NewEnv)) {
                    nameBuilder.Append("N");
                }
                MessageName = nameBuilder.ToString();

                expression = expression.Replace("\r\n", "\n");

                message = host.CreateMessage(out Id, MessageName, expression);
            }
        }
    }
}
