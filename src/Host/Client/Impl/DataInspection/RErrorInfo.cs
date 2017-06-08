// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    internal sealed class RErrorInfo : REvaluationResultInfo, IRErrorInfo {
        public string ErrorText { get; }

        internal RErrorInfo(IRExpressionEvaluator evaluator, string environmentExpression, string expression, string name, string errorText)
            : base(evaluator, environmentExpression, expression, name) {
            ErrorText = errorText;
        }

        public override string ToString() => Invariant($"ERROR: {ErrorText}");

        public override IREvaluationResultInfo ToEnvironmentIndependentResult() =>
            new RErrorInfo(Evaluator, EnvironmentExpression, this.GetEnvironmentIndependentExpression(), Name, ErrorText);
    }
}
