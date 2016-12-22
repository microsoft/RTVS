// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static Microsoft.R.Host.Client.REvaluationResult;

namespace Microsoft.R.DataInspection {
    internal abstract class REvaluationResultInfo : IREvaluationResultInfo {
        public IRExpressionEvaluator Evaluator { get; }

        public string EnvironmentExpression { get; }

        public string Expression { get; }

        public string Name { get; }

        internal REvaluationResultInfo(IRExpressionEvaluator evaluator, string environmentExpression, string expression, string name) {
            Evaluator = evaluator;
            EnvironmentExpression = environmentExpression;
            Expression = expression;
            Name = name;
        }

        internal static REvaluationResultInfo Parse(IRExpressionEvaluator evaluator, string environmentExpression, string name, JObject json) {
            var expression = json.Value<string>(FieldNames.Expression);

            var errorText = json.Value<string>(FieldNames.Error);
            if (errorText != null) {
                return new RErrorInfo(evaluator, environmentExpression, expression, name, errorText);
            }

            var code = json.Value<string>(FieldNames.Promise);
            if (code != null) {
                return new RPromiseInfo(evaluator, environmentExpression, expression, name, code);
            }

            var isActiveBinding = json.Value<bool?>(FieldNames.ActiveBinding);
            if (isActiveBinding == true) {
                return new RActiveBindingInfo(evaluator, environmentExpression, expression, name, json);
            }

            return new RValueInfo(evaluator, environmentExpression, expression, name, json);
        }

        public abstract IREvaluationResultInfo ToEnvironmentIndependentResult();
    }
}
