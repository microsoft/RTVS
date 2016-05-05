// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.ExecutionTracing;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    /// <summary>
    /// Describes the result of evaluating an R expression, with additional metadata such as type information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All properties of this interface, and its derived interfaces such as <see cref="IRValueInfo"/>, implement snapshot
    /// semantics - that is, they have values that describe the result of evaluation at the point where it took place, and do not
    /// reflect any changes to R state that have occurred since then. For methods, however, this can vary; if the method does not
    /// implement snapshot semantics, then its documentation will reflect that.
    /// </para>
    /// <para>
    /// Those methods that do not have snapshot semantics will return fresh results obtained by evaluating the expression that produced
    /// this result in its original context. If the context is no longer available (for example, it was a stack frame that is no
    /// longer there), the results are undefined. 
    /// </remarks>
    public interface IREvaluationInfo {
        IRSession Session { get; }

        /// <summary>
        /// R expression designating the environment in which the evaluation that produced this result took place.
        /// </summary>
        string EnvironmentExpression { get; }

        /// <summary>
        /// R expression that was evaluated to produce this result.
        /// </summary>
        string Expression { get; }

        /// <summary>
        /// Name of the result. This corresponds to the <c>name</c> parameter of <see cref="RTracer.EvaluateAsync"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is filled automatically when the result is produced by <see cref="IRValueInfo.GetChildrenAsync"/>, 
        /// and is primarily useful in that scenario. See the documentation of that method for more information.
        /// </para>
        string Name { get; }
    }

    public static class REvaluationInfoExtensions {
        /// <summary>
        /// Computes the children of this value, and returns a collection of evaluation objects describing each child.
        /// See <see cref="IRExecutionTracer.EvaluateAsync"/> for the meaning of parameters.
        /// </summary>
        /// <param name="maxCount">If not <see langword="null"/>, return at most that many children.</param>
        /// <remarks>
        /// <para>
        /// The resulting collection has an item for every child. If the child could be retrieved, and represents
        /// a value, the corresponding item is an <see cref="IRValueInfo"/> instance. If the child represents
        /// a promise, the promise is not forced, and the item is an <see cref="IRPromiseInfo"/> instance. If the
        /// child represents an active binding, the binding is not evaluated to retrieve the value, and the item is
        /// an <see cref="IRActiveBindingInfo"/> instance. If the child could not be retrieved, the item is an
        /// <see cref="IRErrorInfo"/> instance describing the error that prevented its retrieval.
        /// </para>
        /// <para>
        /// Where order matters (e.g. for children of atomic vectors and lists), children are returned in that order.
        /// Otherwise, the order is undefined. If an object has both ordered and unordered children (e.g. it is a vector
        /// with slots), then it is guaranteed that each group is reported as a contiguous sequence within the returned
        /// collection, and order is honored within each group; but groups themselves are not ordered relative to each other.
        /// </para>
        /// <para>
        /// This method does not respect snapshot semantics - that is, it will re-evaluate the expression that produced
        /// its value, and will obtain the most current list of children, rather than the ones that were there when
        /// the result was originally produced. If the context in which this result was produced is no longer available
        /// (e.g. if it was a stack frame that has since went away), the results are undefined.
        /// </para>
        /// </remarks>
        /// <exception cref="RException">Raised if child retrieval fails.</exception>
        public static async Task<IReadOnlyList<IREvaluationInfo>> DescribeChildrenAsync(
            this IREvaluationInfo info,
            RValueProperties properties,
            int? maxCount = null,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (info.EnvironmentExpression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated environment expression.");
            }
            if (info.Expression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated expression.");
            }

            var call = Invariant($"rtvs:::describe_children({info.Expression.ToRStringLiteral()}, {info.EnvironmentExpression}, {properties.ToRVector()}, {maxCount}, {repr})");
            var jChildren = await info.Session.EvaluateAsync<JArray>(call, REvaluationKind.Normal, cancellationToken);
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
                    var evalResult = REvaluationInfo.Parse(info.Session, info.EnvironmentExpression, name, jEvalResult);
                    children.Add(evalResult);
                }
            }

            return children;
        }

        /// <summary>
        /// If this evaluation result corresponds to an expression that is a valid assignment target (i.e. valid on the
        /// left side of R operator <c>&lt;-</c>, such as a variable), assigns the specified value to that target.
        /// </summary>
        /// <param name="value">Value to assign. Must be a valid R expression.</param>
        /// <returns>
        /// A task that is completed once the assignment completes. Failure to assign is reported as exception on the task.
        /// </returns>
        public static Task AssignAsync(this IREvaluationInfo info, string value, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrEmpty(info.Expression)) {
                throw new InvalidOperationException(Invariant($"{nameof(AssignAsync)} is not supported for this {nameof(REvaluationInfo)} because it doesn't have an associated {nameof(info.Expression)}."));
            }
            return info.Session.ExecuteAsync($"{info.Expression} <- {value}", REvaluationKind.Mutating, cancellationToken);
        }

        /// <summary>
        /// Re-evaluates the expression that was used to create this evaluation object in its original context,
        /// but with a new representation function and properties.
        /// </summary>
        /// <remarks>
        /// Evaluating an expression always produces a regular value, never a promise or an active binding. Thus,
        /// this method can be used to compute the current value of an <see cref="IRActiveBindingInfo"/>, or force
        /// an <see cref="IRPromiseInfo"/>.
        /// </remarks>
        /// <exception cref="RException">Evaluation of the expression produced an error.</exception>
        public static Task<IRValueInfo> GetValueAsync(this IREvaluationInfo info, RValueProperties properties, string repr, CancellationToken cancellationToken = default(CancellationToken)) =>
            info.Session.EvaluateAndDescribeAsync(info.EnvironmentExpression, info.Expression, info.Name, properties, repr, cancellationToken);
    }
}
