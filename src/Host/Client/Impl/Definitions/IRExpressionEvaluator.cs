// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public interface IRExpressionEvaluator {
        /// <summary>
        /// Evaluates an R expression, and returns the result.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="kind">Evaluation flags.</param>
        /// <remarks>
        /// <para>
        /// Evaluation is performed in the global environment (<c>.GlobalEnv</c>) by default; <see cref="REvaluationKind.BaseEnv"/>
        /// or <see cref="REvaluationKind.EmptyEnv"/> can be used to designate a different environment. Evaluation is neither
        /// cancelable nor reentrant by default.
        /// </para>
        /// <para>
        /// If <see cref="REvaluationKind.Json"/> is not specified, the result of evaluation is converted to a character vector
        /// by means of <c>Rf_asChar</c>, and the first element of that vector is returned in <see cref="REvaluationResult.StringResult"/>.
        /// </para>
        /// <para>
        /// If <see cref="REvaluationKind.Json"/> is specified, the result of evaluation is serialized to JSON, as if 
        /// <c>rtvs:::toJSON</c> was invoked on it. If serialization fails, it is a fatal error, and the host process will be
        /// terminated. If it succeeds, the resulting JSON is returned in <see cref="REvaluationResult.Result"/>.
        /// </para>
        /// <para>
        /// If evaluation fails, <see cref="REvaluationResult.ParseStatus"/> and/or <see cref="REvaluationResult.Error"/> will
        /// be set accordingly.
        /// </para>
        /// <para>
        /// This is a low-level evaluation method that requires explicit error checks on the result. It is recommended that
        /// <see cref="RExpressionEvaluatorExtensions.ExecuteAsync"/> is used instead when evakuation is performed solely for
        /// its side effects and no result is expected, and that <see cref="RExpressionEvaluatorExtensions.EvaluateAsync{T}"/>
        /// is used when evaluation produces a JSON value.
        /// </para>
        /// </remarks>
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken));
    }

    public static class RExpressionEvaluatorExtensions {
        /// <summary>
        /// Like <see cref="IRExpressionEvaluator.EvaluateAsync"/>, but takes a <see cref="FormattableString"/> for the expression,
        /// and uses <see cref="CultureInfo.InvariantCulture"/> to format it.
        /// </summary>
        public static Task<REvaluationResult> EvaluateAsync(this IRExpressionEvaluator evaluator, FormattableString expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluator.EvaluateAsync(Invariant(expression), kind, cancellationToken);

        /// <summary>
        /// Like <see cref="IRExpressionEvaluator.EvaluateAsync"/>, but after obtaining the result, deserializes it using
        /// <see cref="JToken.ToObject{T}"/>. If evaluation fails, throws <see cref="REvaluationException"/>.
        /// </summary>
        /// <exception cref="REvaluationException">
        /// <see cref="REvaluationResult.ParseStatus"/> was not <see cref="RParseStatus.OK"/>, or 
        /// <see cref="REvaluationResult.Error"/> was not <see langword="null"/>.
        /// </exception>
        public static async Task<T> EvaluateAsync<T>(this IRExpressionEvaluator evaluator, string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) {
            var res = await evaluator.EvaluateAsync(expression, kind, cancellationToken);
            ThrowOnError(expression, res);
            Trace.Assert(res.Result != null);
            return res.Result.ToObject<T>();
        }

        /// <summary>
        /// Like <see cref="EvaluateAsync{T}(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/>, but takes a
        /// <see cref="FormattableString"/> for the expression, and uses <see cref="CultureInfo.InvariantCulture"/> to format it.
        /// </summary>
        public static Task<T> EvaluateAsync<T>(this IRExpressionEvaluator evaluator, FormattableString expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluator.EvaluateAsync<T>(Invariant(expression), kind, cancellationToken);

        /// <summary>
        /// Like <see cref="EvaluateAsync{T}(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/>, but suppresses
        /// the result, such that it is not trasmitted between the host and the client, and is not serialized or deserialized. 
        /// </summary>
        /// <remarks>
        /// Use in lieu of <see cref="EvaluateAsync{T}(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/> for
        /// evaluations that are performed solely for their side effects, when the result is not inspected.
        /// </remarks>
        public static async Task ExecuteAsync(this IRExpressionEvaluator evaluator, string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) {
            var res = await evaluator.EvaluateAsync(expression, kind | REvaluationKind.NoResult, cancellationToken);
            ThrowOnError(expression, res);
            Trace.Assert(res.Result == null);
        }

        /// <summary>
        /// Like <see cref="ExecuteAsync(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/>, but uses
        /// <see cref="REvaluationKind.Mutating"/> for <c>kind</c>
        /// </summary>
        public static Task ExecuteAsync(this IRExpressionEvaluator evaluator, string expression, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluator.ExecuteAsync(expression, REvaluationKind.Mutating, cancellationToken);

        /// <summary>
        /// Like <see cref="ExecuteAsync(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/>, but takes a
        /// <see cref="FormattableString"/> for the expression, and uses <see cref="CultureInfo.InvariantCulture"/> to format it.
        /// </summary>
        public static Task ExecuteAsync(this IRExpressionEvaluator evaluator, FormattableString expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluator.ExecuteAsync(Invariant(expression), kind, cancellationToken);

        /// <summary>
        /// Like <see cref="ExecuteAsync(IRExpressionEvaluator, FormattableString, REvaluationKind, CancellationToken)"/>, but uses
        /// <see cref="REvaluationKind.Mutating"/> for <c>kind</c>
        /// </summary>
        public static Task ExecuteAsync(this IRExpressionEvaluator evaluator, FormattableString expression, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluator.ExecuteAsync(expression, REvaluationKind.Mutating, cancellationToken);

        private static void ThrowOnError(string expression, REvaluationResult res) {
            if (res.ParseStatus != RParseStatus.OK) {
                throw new REvaluationException(Invariant($"R evaluation failed:\n\n{expression}\n\nExpression could not be parsed; ParseStatus={res.ParseStatus}"));
            } else if (res.Error != null) {
                throw new REvaluationException(Invariant($"R evaluation failed:\n\n{expression}\n\n{res.Error}"));
            }
        }
    }
}

