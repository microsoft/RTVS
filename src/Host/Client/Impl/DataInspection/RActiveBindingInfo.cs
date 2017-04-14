// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static Microsoft.R.Host.Client.REvaluationResult;

namespace Microsoft.R.DataInspection {
    internal sealed class RActiveBindingInfo : REvaluationResultInfo, IRActiveBindingInfo {
        public IRValueInfo ComputedValue { get; }

        internal RActiveBindingInfo(IRExpressionEvaluator evaluator, string environmentExpression, string expression, string name, JObject json)
            : this(evaluator, environmentExpression, expression, name, (IRValueInfo)null) {

            JObject bindingResultJson = json.Value<JObject>(FieldNames.ComputedValue);
            if (bindingResultJson != null) {
                ComputedValue = new RValueInfo(evaluator, environmentExpression, expression, name, bindingResultJson);
            }
        }

        internal RActiveBindingInfo(IRExpressionEvaluator evaluator, string environmentExpression, string expression, string name, IRValueInfo computedValue)
            : base(evaluator, environmentExpression, expression, name) {

            ComputedValue = computedValue;
        }

        public override IREvaluationResultInfo ToEnvironmentIndependentResult() =>
            new RActiveBindingInfo(Evaluator, EnvironmentExpression, this.GetEnvironmentIndependentExpression(), Name, ComputedValue);
    }
}
