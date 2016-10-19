// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    internal sealed class RPromiseInfo : REvaluationResultInfo, IRPromiseInfo {
        public string Code { get; }

        internal RPromiseInfo(IRSession session, string environmentExpression, string expression, string name, string code)
            : base(session, environmentExpression, expression, name) {
            Code = code;
        }

        public override string ToString() {
            return Invariant($"PROMISE: {Code}");
        }

        public override IREvaluationResultInfo ToEnvironmentIndependentResult() =>
            new RPromiseInfo(Session, EnvironmentExpression, this.GetEnvironmentIndependentExpression(), Name, Code);
    }
}
