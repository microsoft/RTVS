// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static Microsoft.R.Host.Client.REvaluationResult;

namespace Microsoft.R.DataInspection {
    internal abstract class REvaluationResultInfo : IREvaluationResultInfo {
        public IRSession Session { get; }

        public string EnvironmentExpression { get; }

        public string Expression { get; }

        public string Name { get; }

        internal REvaluationResultInfo(IRSession session, string environmentExpression, string expression, string name) {
            Session = session;
            EnvironmentExpression = environmentExpression;
            Expression = expression;
            Name = name;
        }

        internal static REvaluationResultInfo Parse(IRSession session, string environmentExpression, string name, JObject json) {
            var expression = json.Value<string>(FieldNames.Expression);

            var errorText = json.Value<string>(FieldNames.Error);
            if (errorText != null) {
                return new RErrorInfo(session, environmentExpression, expression, name, errorText);
            }

            var code = json.Value<string>(FieldNames.Promise);
            if (code != null) {
                return new RPromiseInfo(session, environmentExpression, expression, name, code);
            }

            var isActiveBinding = json.Value<bool?>(FieldNames.ActiveBinding);
            if (isActiveBinding == true) {
                return new RActiveBindingInfo(session, environmentExpression, expression, name, json);
            }

            return new RValueInfo(session, environmentExpression, expression, name, json);
        }

        public abstract IREvaluationResultInfo ToEnvironmentIndependentResult();
    }
}
