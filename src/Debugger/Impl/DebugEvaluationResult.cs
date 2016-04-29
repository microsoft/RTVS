// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// Describes the result of evaluating an R expression, with additional metadata such as type information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All properties of this class, and its derived classes such as <see cref="DebugValueEvaluationResult"/>, implement snapshot
    /// semantics - that is, they have values that describe the result of evaluation at the point where it took place, and do not
    /// reflect any changes to R state that have occurred since then. For methods, however, this can vary; if the method does not
    /// implement snapshot semantics, then its documentation will reflect that.
    /// </para>
    /// <para>
    /// Those methods that do not have snapshot semantics will return fresh results obtained by evaluating the expression that produced
    /// this result in its original context. If the context is no longer available (for example, it was a stack frame that is no
    /// longer there), the results are undefined. 
    /// </remarks>
    public abstract class DebugEvaluationResult : IDebugEvaluationResult {
        #region IDebugEvaluationResult
        public DebugSession Session { get; }

        /// <summary>
        /// R expression designating the environment in which the evaluation that produced this result took place.
        /// </summary>
        public string EnvironmentExpression { get; }

        /// <summary>
        /// R expression that was evaluated to produce this result.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Name of the result. This corresponds to the <c>name</c> parameter of <see cref="DebugSession.EvaluateAsync"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is filled automatically when the result is produced by <see cref="DebugValueEvaluationResult.GetChildrenAsync"/>, 
        /// and is primarily useful in that scenario. See the documentation of that method for more information.
        /// </para>
        public string Name { get; }
        #endregion

        internal DebugEvaluationResult(DebugSession session, string environmentExpression, string expression, string name) {
            Session = session;
            EnvironmentExpression = environmentExpression;
            Expression = expression;
            Name = name;
        }

        internal static DebugEvaluationResult Parse(DebugSession session, string environmentExpression, string name, JObject json) {
            var expression = json.Value<string>("expression");

            var errorText = json.Value<string>("error");
            if (errorText != null) {
                return new DebugErrorEvaluationResult(session, environmentExpression, expression, name, errorText);
            }

            var code = json.Value<string>("promise");
            if (code != null) {
                return new DebugPromiseEvaluationResult(session, environmentExpression, expression, name, code);
            }

            var isActiveBinding = json.Value<bool?>("active_binding");
            if (isActiveBinding == true) {
                return new DebugActiveBindingEvaluationResult(session, environmentExpression, expression, name);
            }

            return new DebugValueEvaluationResult(session, environmentExpression, expression, name, json);
        }

        /// <summary>
        /// If this evaluation result corresponds to an expression that is a valid assignment target (i.e. valid on the
        /// left side of R operator <c>&lt;-</c>, such as a variable), assigns the specified value to that target.
        /// </summary>
        /// <param name="value">Value to assign. Must be a valid R expression.</param>
        /// <returns>
        /// A task that is completed once the assignment completes. Failure to assign is reported as exception on the task.
        /// </returns>
        public Task SetValueAsync(string value, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrEmpty(Expression)) {
                throw new InvalidOperationException(Invariant($"{nameof(SetValueAsync)} is not supported for this {nameof(DebugEvaluationResult)} because it doesn't have an associated {nameof(Expression)}."));
            }
            return Session.RSession.ExecuteAsync($"{Expression} <- {value}", REvaluationKind.Mutating, cancellationToken);
        }

        /// <summary>
        /// Re-evaluates the expression that produced this result in its original context, and produces a new result.
        /// <see cref="DebugSession.EvaluateAsync"/> for description of parameters.
        /// </summary>
        /// <remarks>
        /// This is used primarily to evaluate the result with a different set of <see cref="DebugEvaluationResultFields"/>,
        /// or different <c>repr</c>, to load additional data on demand.
        /// </remarks>
        public Task<DebugEvaluationResult> EvaluateAsync(
            DebugEvaluationResultFields fields,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            if (EnvironmentExpression == null) {
                throw new InvalidOperationException("Cannot re-evaluate an evaluation result that does not have an associated environment expression.");
            }
            if (Expression == null) {
                throw new InvalidOperationException("Cannot re-evaluate an evaluation result that does not have an associated expression.");
            }
            return Session.EvaluateAsync(EnvironmentExpression, Expression, Name, fields, repr, cancellationToken);
        }

        public override bool Equals(object obj) =>
            base.Equals(obj) || (obj as IEquatable<DebugEvaluationResult>)?.Equals(this) == true;

        public override int GetHashCode() =>
            base.GetHashCode();
    }
}
