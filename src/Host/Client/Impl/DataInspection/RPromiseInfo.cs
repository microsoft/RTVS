// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    internal sealed class RPromiseInfo : REvaluationResultInfo, IRPromiseInfo {
        public string Code { get; }

        internal RPromiseInfo(IRExpressionEvaluator evaluator, string environmentExpression, string expression, string name, string code)
            : base(evaluator, environmentExpression, expression, name) {
            Code = code;
        }

        public override string ToString() => Invariant($"PROMISE: {Code}");

        public override IREvaluationResultInfo ToEnvironmentIndependentResult() =>
            new RPromiseInfo(Evaluator, EnvironmentExpression, this.GetEnvironmentIndependentExpression(), Name, Code);
    }
}
