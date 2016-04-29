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
    internal abstract class REvaluationInfo : IREvaluationInfo {
        public IRSession Session { get; }

        public string EnvironmentExpression { get; }

        public string Expression { get; }

        public string Name { get; }

        internal REvaluationInfo(IRSession session, string environmentExpression, string expression, string name) {
            Session = session;
            EnvironmentExpression = environmentExpression;
            Expression = expression;
            Name = name;
        }

        internal static REvaluationInfo Parse(IRSession session, string environmentExpression, string name, JObject json) {
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

        public Task SetValueAsync(string value, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrEmpty(Expression)) {
                throw new InvalidOperationException(Invariant($"{nameof(SetValueAsync)} is not supported for this {nameof(REvaluationInfo)} because it doesn't have an associated {nameof(Expression)}."));
            }
            return Session.ExecuteAsync($"{Expression} <- {value}", REvaluationKind.Mutating, cancellationToken);
        }

        public async Task<IReadOnlyList<IREvaluationInfo>> DescribeChildrenAsync(
            RValueProperties fields,
            int? maxCount = null,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (EnvironmentExpression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated environment expression.");
            }
            if (Expression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated expression.");
            }

            var call = Invariant($"rtvs:::describe_children({Expression.ToRStringLiteral()}, {EnvironmentExpression}, {fields.ToRVector()}, {maxCount}, {repr})");
            var jChildren = await Session.EvaluateAsync<JArray>(call, REvaluationKind.Normal, cancellationToken);
            Trace.Assert(
                jChildren.Children().All(t => t is JObject),
                Invariant($"rtvs:::describe_children(): object of objects expected.\n\n{jChildren}"));

            var children = new List<REvaluationInfo>();
            foreach (var child in jChildren) {
                var childObject = (JObject)child;
                Trace.Assert(
                    childObject.Count == 1,
                    Invariant($"rtvs:::describe_children(): each object is expected contain one object\n\n"));
                foreach (var kv in childObject) {
                    var name = kv.Key;
                    var jEvalResult = (JObject)kv.Value;
                    var evalResult = Parse(Session, EnvironmentExpression, name, jEvalResult);
                    children.Add(evalResult);
                }
            }

            return children;
        }

        public override bool Equals(object obj) =>
            base.Equals(obj) || (obj as IEquatable<REvaluationInfo>)?.Equals(this) == true;

        public override int GetHashCode() =>
            base.GetHashCode();
    }
}
