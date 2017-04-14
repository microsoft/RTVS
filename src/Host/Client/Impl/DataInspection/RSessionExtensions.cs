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
    public static class RSessionExtensions {
        /// <summary>
        /// Like <see cref="TryEvaluateAndDescribeAsync(IRExpressionEvaluator, string, string, string, REvaluationResultProperties, string, CancellationToken)"/>,
        /// but evaluates in the global environment (<c>.GlobalEnv</c>), and the result is not named.
        /// </summary>
        public static Task<IREvaluationResultInfo> TryEvaluateAndDescribeAsync(
            this IRExpressionEvaluator session,
            string expression,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            session.TryEvaluateAndDescribeAsync(REnvironments.GlobalEnv, expression, null, properties, repr, cancellationToken);

        /// <summary>
        /// Evaluates an R expresion in the specified environment, and returns an object describing the result.
        /// </summary>
        /// <param name="environmentExpression">
        /// R expression designating the environment in which <paramref name="expression"/> will be evaluated.
        /// </param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="name"><see cref="IREvaluationResultInfo.Name"/> of the returned evaluation result.</param>
        /// <param name="properties">Specifies which <see cref="IREvaluationResultInfo"/> properties should be present in the result.</param>
        /// <param name="repr">
        /// An R expression that must evaluate to a function that takes an R value as its sole argument, and returns the
        /// string representation of that argument as a single-element character vector. The representation is stored in
        /// <see cref="IRValueInfo.Representation"/> property of the produced result. If this argument is
        /// <see langword="null"/>, no representation is computed, and <see cref="IRValueInfo.Representation"/>
        /// will also be <see langword="null"/>.
        /// Use helper properties and methods in <see cref="RValueRepresentations"/> to obtain an appropriate expression
        /// for standard R functions such as <c>deparse()</c> or <c>str()</c>.
        /// </param>
        /// <remarks>
        /// <returns>
        /// If evaluation succeeded, an instance of <see cref="IRValueInfo"/> describing the resulting value.
        /// If evaluation failed with an error, an instance of <see cref="IRErrorInfo"/> describing the error.
        /// This method never returns <see cref="IRActiveBindingInfo"/> or <see cref="IRPromiseInfo"/>.
        /// </returns>
        public static async Task<IREvaluationResultInfo> TryEvaluateAndDescribeAsync(
            this IRExpressionEvaluator session,
            string environmentExpression,
            string expression,
            string name,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            if (environmentExpression == null) {
                throw new ArgumentNullException(nameof(environmentExpression));
            }
            if (expression == null) {
                throw new ArgumentNullException(nameof(expression));
            }

            await TaskUtilities.SwitchToBackgroundThread();

            environmentExpression = environmentExpression ?? "NULL";
            var code = Invariant($"rtvs:::eval_and_describe({expression.ToRStringLiteral()}, ({environmentExpression}),, {properties.ToRVector()},, {repr})");
            var result = await session.EvaluateAsync<JObject>(code, REvaluationKind.Normal, cancellationToken);
            return REvaluationResultInfo.Parse(session, environmentExpression, name, result);
        }

        /// <summary>
        /// Like <see cref="TryEvaluateAndDescribeAsync(IRSession, string, REvaluationResultProperties, string, CancellationToken)"/>,
        /// but throws <see cref="RException"/> if result is an <see cref="IRErrorInfo"/>.
        /// </summary>
        public static Task<IRValueInfo> EvaluateAndDescribeAsync(
            this IRExpressionEvaluator session,
            string expression,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            session.EvaluateAndDescribeAsync(REnvironments.GlobalEnv, expression, null, properties, repr, cancellationToken);

        /// <summary>
        /// Like <see cref="TryEvaluateAndDescribeAsync(IRSession, string, string, string, REvaluationResultProperties, string, CancellationToken)"/>,
        /// but throws <see cref="RException"/> if result is an <see cref="IRErrorInfo"/>.
        /// </summary>
        public static async Task<IRValueInfo> EvaluateAndDescribeAsync(
            this IRExpressionEvaluator session,
            string environmentExpression,
            string expression,
            string name,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            var info = await session.TryEvaluateAndDescribeAsync(environmentExpression, expression, name, properties, repr, cancellationToken);

            var error = info as IRErrorInfo;
            if (error != null) {
                throw new RException(error.ErrorText);
            }

            Debug.Assert(info is IRValueInfo);
            return (IRValueInfo)info;
        }

        /// <summary>
        /// Computes the children of the object represented by the given expression, and returns a collection of
        /// evaluation objects describing each child.
        /// See <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/> for the meaning of other parameters.
        /// </summary>
        /// <param name="evaluateActiveBindings">Passes a flag to R to evalaute bindings based on RTools Settings.</param>
        /// <param name="maxCount">If not <see langword="null"/>, return at most that many children.</param>
        /// <remarks>
        /// <para>
        /// The resulting collection has an item for every child. If the child could be retrieved, and represents
        /// a value, the corresponding item is an <see cref="IRValueInfo"/> instance. If the child represents
        /// a promise, the promise is not forced, and the item is an <see cref="IRPromiseInfo"/> instance. If the
        /// child represents an active binding, the binding may be evaluated to retrieve the value, and the item is
        /// an <see cref="IRActiveBindingInfo"/> instance. If the child could not be retrieved, the item is an 
        /// <see cref="IRErrorInfo"/> instance describing the error that prevented its retrieval.
        /// </para>
        /// <para>
        /// Where order matters (e.g. for children of atomic vectors and lists), children are returned in that order.
        /// Otherwise, the order is undefined. If an object has both ordered and unordered children (e.g. it is a vector
        /// with slots), then it is guaranteed that each group is reported as a contiguous sequence within the returned
        /// collection, and order is honored within each group; but groups themselves are not ordered relative to each other.
        /// </para>
        /// </remarks>
        /// <exception cref="RException">
        /// Raised if the operation fails as a whole (note that if only specific children cannot be retrieved, those
        /// children are represented by <see cref="IRErrorInfo"/> instances in the returned collection instead).
        /// </exception>
        public static async Task<IReadOnlyList<IREvaluationResultInfo>> DescribeChildrenAsync(
            this IRExpressionEvaluator session,
            string environmentExpression,
            string expression,
            REvaluationResultProperties properties,
            string repr,
            int? maxCount = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            await TaskUtilities.SwitchToBackgroundThread();

            var call = Invariant($"rtvs:::describe_children({expression.ToRStringLiteral()}, {environmentExpression}, {properties.ToRVector()}, {maxCount}, {repr})");
            var jChildren = await session.EvaluateAsync<JArray>(call, REvaluationKind.Normal, cancellationToken);
            Trace.Assert(
                jChildren.Children().All(t => t is JObject),
                Invariant($"rtvs:::describe_children(): object of objects expected.\n\n{jChildren}"));

            var children = new List<REvaluationResultInfo>();
            foreach (var child in jChildren) {
                var childObject = (JObject)child;
                Trace.Assert(
                    childObject.Count == 1,
                    Invariant($"rtvs:::describe_children(): each object is expected contain one object\n\n"));
                foreach (var kv in childObject) {
                    var name = kv.Key;
                    var jEvalResult = (JObject)kv.Value;
                    var evalResult = REvaluationResultInfo.Parse(session, environmentExpression, name, jEvalResult);
                    children.Add(evalResult);
                }
            }

            return children;
        }
    }
}