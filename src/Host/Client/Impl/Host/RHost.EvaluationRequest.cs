// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class EvaluationRequest : BaseRequest {
            public readonly string Expression;
            public readonly REvaluationKind Kind;
            public readonly TaskCompletionSourceEx<REvaluationResult> CompletionSource = new TaskCompletionSourceEx<REvaluationResult>();

            private EvaluationRequest(string id, string messageName, string expression, REvaluationKind kind) : base (id, messageName) {
                Expression = expression;
                Kind = kind;
            }

            public static EvaluationRequest Create(RHost host, string expression, REvaluationKind kind, out JArray message) {
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
                if (kind.HasFlag(REvaluationKind.Raw)) {
                    nameBuilder.Append('r');
                }
                string messageName = nameBuilder.ToString();

                expression = expression.Replace("\r\n", "\n");

                string id;
                message = host.CreateMessage(host.CreateMessageHeader(out id, messageName, null), expression);

                return new EvaluationRequest(id, messageName, expression, kind);
            }
        }
    }
}
