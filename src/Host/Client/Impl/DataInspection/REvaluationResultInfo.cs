// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

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
            var expression = json.Value<string>("expression");

            var errorText = json.Value<string>("error");
            if (errorText != null) {
                return new RErrorInfo(session, environmentExpression, expression, name, errorText);
            }

            var code = json.Value<string>("promise");
            if (code != null) {
                return new RPromiseInfo(session, environmentExpression, expression, name, code);
            }

            var isActiveBinding = json.Value<bool?>("active_binding");
            if (isActiveBinding == true) {
                return new RActiveBindingInfo(session, environmentExpression, expression, name);
            }

            return new RValueInfo(session, environmentExpression, expression, name, json);
        }
    }
}
