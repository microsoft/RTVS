// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    public static class RSessionExtensions {
        /// <summary>
        /// Like <see cref="TryEvaluateAndDescribeAsync(IRSession, string, string, string, RValueProperties, string, CancellationToken)"/>,
        /// but evaluates in the global environment (<c>.GlobalEnv</c>), and the result is not named.
        /// </summary>
        public static Task<IREvaluationInfo> TryEvaluateAndDescribeAsync(
            this IRSession session,
            string expression,
            RValueProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            session.TryEvaluateAndDescribeAsync("base::.GlobalEnv", expression, null, properties, repr, cancellationToken);

        /// <summary>
        /// Evaluates an R expresion in the specified environment, and returns an object describing the result.
        /// </summary>
        /// <param name="environmentExpression">
        /// R expression designating the environment in which <paramref name="expression"/> will be evaluated.
        /// </param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="name"><see cref="IREvaluationInfo.Name"/> of the returned evaluation result.</param>
        /// <param name="properties">Specifies which <see cref="IREvaluationInfo"/> properties should be present in the result.</param>
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
        public static async Task<IREvaluationInfo> TryEvaluateAndDescribeAsync(
            this IRSession session,
            string environmentExpression,
            string expression,
            string name,
            RValueProperties properties,
            string repr = null,
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
            var result = await session.EvaluateAsync<JObject>(code, REvaluationKind.Json, cancellationToken);
            return REvaluationInfo.Parse(session, environmentExpression, name, result);
        }

        /// <summary>
        /// Like <see cref="TryEvaluateAndDescribeAsync(IRSession, string, RValueProperties, string, CancellationToken)"/>,
        /// but throws <see cref="RException"/> if result is an <see cref="IRErrorInfo"/>.
        /// </summary>
        public static Task<IRValueInfo> EvaluateAndDescribeAsync(
            this IRSession session,
            string expression,
            RValueProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            session.EvaluateAndDescribeAsync("base::.GlobalEnv", expression, null, properties, repr, cancellationToken);

        /// <summary>
        /// Like <see cref="TryEvaluateAndDescribeAsync(IRSession, string, string, string, RValueProperties, string, CancellationToken)"/>,
        /// but throws <see cref="RException"/> if result is an <see cref="IRErrorInfo"/>.
        /// </summary>
        public static async Task<IRValueInfo> EvaluateAndDescribeAsync(
            this IRSession session,
            string environmentExpression,
            string expression,
            string name,
            RValueProperties properties,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            var info = await session.TryEvaluateAndDescribeAsync(environmentExpression, expression, name, properties, repr, cancellationToken);

            var error = info as IRErrorInfo;
            if (error != null) {
                throw new RException(error.ErrorText);
            }

            Debug.Assert(error is IRValueInfo);
            return (IRValueInfo)error;
        }
    }
}