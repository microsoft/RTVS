// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
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
    public interface IREvaluationResultInfo {
        IRExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// R expression designating the environment in which the evaluation that produced this result took place.
        /// </summary>
        string EnvironmentExpression { get; }

        /// <summary>
        /// R expression that was evaluated to produce this result.
        /// </summary>
        string Expression { get; }

        /// <summary>
        /// Name of the result. This corresponds to the <c>name</c> parameter of <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is filled automatically when the result is produced by <see cref="IRValueInfo.GetChildrenAsync"/>, 
        /// and is primarily useful in that scenario. See the documentation of that method for more information.
        /// </para>
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Creates a copy of this result that can be evaluated in any environment (rather than just the one designated by
        /// <see cref="EnvironmentExpression"/>) to produce the same value.
        /// </summary>
        /// <remarks>
        /// No evaluation takes place. The new result is identical to the one on which the method was called, except that
        /// <see cref="EnvironmentExpression"/> is incorporated directly into <see cref="Expression"/>. 
        /// </remarks>
        IREvaluationResultInfo ToEnvironmentIndependentResult();
    }

    public static class REvaluationResultInfoExtensions {
        /// <summary>
        /// Like <see cref="RSessionExtensions.DescribeChildrenAsync"/>, but returns children of the object described
        /// by the provided evaluation info.
        /// </summary>
        public static Task<IReadOnlyList<IREvaluationResultInfo>> DescribeChildrenAsync(
            this IREvaluationResultInfo info,
            REvaluationResultProperties properties,
            string repr,
            int? maxCount = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            info.Evaluator.DescribeChildrenAsync(info.EnvironmentExpression, info.Expression, properties, repr, maxCount, cancellationToken);

        /// <summary>
        /// If this evaluation result corresponds to an expression that is a valid assignment target (i.e. valid on the
        /// left side of R operator <c>&lt;-</c>, such as a variable), assigns the specified value to that target.
        /// </summary>
        /// <param name="value">Value to assign. Must be a valid R expression.</param>
        /// <returns>
        /// A task that is completed once the assignment completes. Failure to assign is reported as exception on the task.
        /// </returns>
        public static Task AssignAsync(this IREvaluationResultInfo info, string value, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrEmpty(info.Expression)) {
                throw new InvalidOperationException(Invariant($"{nameof(AssignAsync)} is not supported for this {nameof(REvaluationResultInfo)} because it doesn't have an associated {nameof(info.Expression)}."));
            }
            return info.Evaluator.ExecuteAsync(Invariant($"{info.Expression} <- {value}"), cancellationToken);
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
        public static Task<IRValueInfo> GetValueAsync(this IREvaluationResultInfo info, REvaluationResultProperties properties, string repr, CancellationToken cancellationToken = default(CancellationToken)) =>
            info.Evaluator.EvaluateAndDescribeAsync(info.EnvironmentExpression, info.Expression, info.Name, properties, repr, cancellationToken);

        /// <summary>
        /// Computes the expression that can be used to produce the same value in any environment.
        /// </summary>
        public static string GetEnvironmentIndependentExpression(this IREvaluationResultInfo info) =>
            info.EnvironmentExpression + "$" + info.Expression;
    }
}
