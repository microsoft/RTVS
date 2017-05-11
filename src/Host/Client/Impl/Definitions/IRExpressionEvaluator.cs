// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public interface IRExpressionEvaluator {
        /// <summary>
        /// Evaluates an R expression, and returns the result.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="kind">Evaluation flags.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <remarks>
        /// <para>
        /// Evaluation is performed in the global environment (<c>.GlobalEnv</c>) by default; <see cref="REvaluationKind.BaseEnv"/>
        /// or <see cref="REvaluationKind.EmptyEnv"/> can be used to designate a different environment. Evaluation is neither
        /// cancelable nor reentrant by default.
        /// </para>
        /// <para>
        /// The result of evaluation is serialized to JSON, as if <c>rtvs:::toJSON</c> was invoked on it. If serialization fails,
        /// it is a fatal error, and the host process will be terminated. If it succeeds, the resulting JSON is returned in
        /// <see cref="REvaluationResult.Result"/>.
        /// </para>
        /// <para>
        /// If evaluation fails, <see cref="REvaluationResult.ParseStatus"/> and/or <see cref="REvaluationResult.Error"/> will
        /// be set accordingly.
        /// </para>
        /// <para>
        /// This is a low-level evaluation method that requires explicit error checks on the result. It is recommended that
        /// <see cref="RExpressionEvaluatorExtensions.ExecuteAsync"/> is used instead when evaluation is performed solely for
        /// its side effects and no result is expected, and that <see cref="RExpressionEvaluatorExtensions.EvaluateAsync{T}"/>
        /// is used when evaluation produces a JSON value.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A task that represents evaluation. If evaluation is completed successfully, <see cref="Task.Result"/> will return
        /// <see cref="REvaluationResult"/> that contains response from R host process. If R Host get terminated or connection
        /// to the remote host is lost, task will be <see cref="TaskStatus.Canceled"/> with <see cref="RHostDisconnectedException"/>.
        /// If <paramref name="cancellationToken"/> become canceled before method is completed, task will be <see cref="TaskStatus.Canceled"/>
        /// with <see cref="OperationCanceledException"/>. 
        /// </returns>
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
        /// <para>
        /// Like <see cref="IRExpressionEvaluator.EvaluateAsync"/>, but after obtaining the result, deserializes it. 
        /// </para>
        /// <para>
        /// If <typeparamref name="T"/> is an array of bytes, <see cref="REvaluationKind.RawResult"/> is implicitly added to <paramref name="kind"/>,
        /// and <see cref="REvaluationResult.RawResult"/> produced by the evaluation is returned as is.
        /// </para>
        /// <para>
        /// If <typeparamref name="T"/> is any other type, <see cref="JToken.ToObject{T}"/> is used to convert <see cref="REvaluationResult.Result"/>
        /// to <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// If evaluation fails, returned task will be <see cref="TaskStatus.Faulted"/> with <see cref="REvaluationException"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// A task that represents evaluation. If evaluation is completed successfully, <see cref="Task.Result"/> will return
        /// deserialized object that represents response from R host process. If R Host get terminated or connection
        /// to the remote host is lost, task will be <see cref="TaskStatus.Canceled"/> with <see cref="RHostDisconnectedException"/>.
        /// If <paramref name="cancellationToken"/> become canceled before method is completed, task will be <see cref="TaskStatus.Canceled"/>
        /// with <see cref="OperationCanceledException"/>. If <see cref="REvaluationResult.ParseStatus"/> was not <see cref="RParseStatus.OK"/>, or 
        /// <see cref="REvaluationResult.Error"/> was not <see langword="null"/>, task will be <see cref="TaskStatus.Faulted"/> with <see cref="REvaluationException"/>.
        /// </returns>
        public static async Task<T> EvaluateAsync<T>(this IRExpressionEvaluator evaluator, string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) {
            bool isRaw = typeof(T) == typeof(byte[]);
            if (isRaw) {
                kind |= REvaluationKind.RawResult;
            }

            var res = await evaluator.EvaluateAsync(expression, kind, cancellationToken);
            ThrowOnError(expression, res);

            if (isRaw) {
                Trace.Assert(res.RawResult != null);
                return (T)(object)res.RawResult;
            } else {
                Trace.Assert(res.Result != null);
                return res.Result.ToObject<T>();
            }
        }

        /// <summary>
        /// Like <see cref="EvaluateAsync{T}(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/>, but takes a
        /// <see cref="FormattableString"/> for the expression, and uses <see cref="CultureInfo.InvariantCulture"/> to format it.
        /// </summary>
        public static Task<T> EvaluateAsync<T>(this IRExpressionEvaluator evaluator, FormattableString expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluator.EvaluateAsync<T>(Invariant(expression), kind, cancellationToken);

        /// <summary>
        /// Like <see cref="EvaluateAsync{T}(IRExpressionEvaluator, string, REvaluationKind, CancellationToken)"/>, but suppresses
        /// the result, such that it is not transmitted between the host and the client, and is not serialized or deserialized. 
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

