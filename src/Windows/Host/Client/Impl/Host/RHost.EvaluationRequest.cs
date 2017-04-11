// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class EvaluationRequest : Request<REvaluationResult> {
            private static readonly string[] parseStatusNames = { "NULL", "OK", "INCOMPLETE", "ERROR", "EOF" };

            public readonly REvaluationKind Kind;

            private EvaluationRequest(RHost host, Message message, REvaluationKind kind, CancellationToken cancellationToken)
                : base(host, message, cancellationToken) {
                Kind = kind;
            }

            public static async Task<EvaluationRequest> SendAsync(RHost host, string expression, REvaluationKind kind, CancellationToken cancellationToken) {
                var nameBuilder = new StringBuilder("?=");
                if (kind.HasFlag(REvaluationKind.Reentrant)) {
                    nameBuilder.Append('@');
                }
                if (kind.HasFlag(REvaluationKind.Cancelable)) {
                    nameBuilder.Append('/');
                }
                if (kind.HasFlag(REvaluationKind.BaseEnv)) {
                    nameBuilder.Append('B');
                }
                if (kind.HasFlag(REvaluationKind.EmptyEnv)) {
                    nameBuilder.Append('E');
                }
                if (kind.HasFlag(REvaluationKind.NoResult)) {
                    nameBuilder.Append('0');
                }
                if (kind.HasFlag(REvaluationKind.RawResult)) {
                    nameBuilder.Append('r');
                }
                string messageName = nameBuilder.ToString();

                expression = expression.Replace("\r\n", "\n");

                var message = host.CreateMessage(messageName, ulong.MaxValue, new JArray { expression });
                var request = new EvaluationRequest(host, message, kind, cancellationToken);

                await host.SendAsync(message, cancellationToken);
                return request;
            }

            public override void Handle(RHost host, Message response) {
                response.ExpectArguments(1, 3);
                var firstArg = response[0] as JValue;
                if (firstArg != null && firstArg.Value == null) {
                    CompletionSource.TrySetCanceled();
                    return;
                }

                response.ExpectArguments(3);
                var parseStatus = response.GetEnum<RParseStatus>(0, "parseStatus", parseStatusNames);
                var error = response.GetString(1, "error", allowNull: true);

                REvaluationResult result;
                if (Kind.HasFlag(REvaluationKind.NoResult)) {
                    result = new REvaluationResult(error, parseStatus);
                } else if (Kind.HasFlag(REvaluationKind.RawResult)) {
                    result = new REvaluationResult(response.Blob, error, parseStatus);
                } else {
                    result = new REvaluationResult(response[2], error, parseStatus);
                }

                CompletionSource.TrySetResult(result);
            }
        }
    }
}
