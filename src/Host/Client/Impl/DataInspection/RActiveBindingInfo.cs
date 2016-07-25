// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.DataInspection {
    internal sealed class RActiveBindingInfo : REvaluationResultInfo, IRActiveBindingInfo {
        public IRValueInfo ComputedValue { get; }

        internal RActiveBindingInfo(IRSession session, string environmentExpression, string expression, string name, JObject json)
            : base(session, environmentExpression, expression, name) {
            JObject bindingResultJson = json.Value<JObject>(REvaluationResultFieldNames.ComputedValueFieldName);
            if(bindingResultJson == null) {
                ComputedValue = null;
            } else {
                ComputedValue = new RValueInfo(session, environmentExpression, expression, name, bindingResultJson);
            }    
        }
    }
}
