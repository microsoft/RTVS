// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static Microsoft.R.Host.Client.REvaluationResult;

namespace Microsoft.R.DataInspection {
    internal sealed class RActiveBindingInfo : REvaluationResultInfo, IRActiveBindingInfo {
        public IRValueInfo ComputedValue { get; }

        internal RActiveBindingInfo(IRSession session, string environmentExpression, string expression, string name, JObject json)
            : this(session, environmentExpression, expression, name, (IRValueInfo)null) {

            JObject bindingResultJson = json.Value<JObject>(FieldNames.ComputedValue);
            if (bindingResultJson != null) {
                ComputedValue = new RValueInfo(session, environmentExpression, expression, name, bindingResultJson);
            }
        }

        internal RActiveBindingInfo(IRSession session, string environmentExpression, string expression, string name, IRValueInfo computedValue)
            : base(session, environmentExpression, expression, name) {

            ComputedValue = computedValue;
        }

        public override IREvaluationResultInfo ToEnvironmentIndependentResult() =>
            new RActiveBindingInfo(Session, EnvironmentExpression, this.GetEnvironmentIndependentExpression(), Name, ComputedValue);
    }
}
