// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.DataInspection {
    internal sealed class RActiveBindingInfo : REvaluationResultInfo, IRActiveBindingInfo {
        public IRValueInfo Value { get; }

        internal RActiveBindingInfo(IRSession session, string environmentExpression, string expression, string name, JObject json)
            : base(session, environmentExpression, expression, name) {
            JObject bindingResultJson = json.Value<JObject>("binding_result");
            if(bindingResultJson == null) {
                Value = null;
            } else {
                Value = new RValueInfo(session, environmentExpression, expression, name, bindingResultJson);
            }    
        }
    }
}
