// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.StackTracing {
    /// <summary>
    /// Describes an R stack frame.
    /// </summary>
    /// <remarks>
    /// Properties of this class represent the state of the frame as it was at the point <see cref="RSessionExtensions.TracebackAsync"/>
    /// was called, and do not necessarily correspond to the current state if it has changed since then.
    /// </remarks>
    public interface IRStackFrame {

        IRSession Session { get; }

        /// <summary>
        /// Index of this frame in the call stack. Frames are counted from the bottom of the stack upward,
        /// starting from 0 (which is normally <c>.GlobalEnv</c>).
        /// </summary>
        /// <remarks>
        /// Corresponds to the <c>which</c> parameter of <c>sys.frame()</c> R function.
        /// </remarks>
        int Index { get; }

        /// <summary>
        /// An R expression that, when evaluated, will produce 
        /// </summary>
        string EnvironmentExpression { get; }

        /// <summary>
        /// Name of R environment corresponding to this stack frame, or <c>null</c> if it doesn't have a name.
        /// </summary>
        /// <remarks>
        /// This is <em>not</em> the same as <c>environmentName()</c> function in R. Rather, it corresponds to <c>format()</c>.
        /// For example, the name of the global environment is <c>"&lt;environment: R_GlobalEnv&gt;"</c>, and the name of the
        /// base package environment is <c>"&lt;environment: package:utils&gt;"</c>.
        /// </remarks>
        string EnvironmentName { get; }

        /// <summary>
        /// The stack frame that contains the call that produced this frame (i.e. the frame right below this one on the stack).
        /// </summary>
        IRStackFrame CallingFrame { get; }

        /// <summary>
        /// Name of the file in which the currently executing line of code in this frame is located, or <c>null</c> if this
        /// information is not available.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Line number (1-based) of currently executing line of code in this frame is located, or <c>null</c> if this
        /// information is not available.
        /// </summary>
        int? LineNumber { get; }

        /// <summary>
        /// Currently executing call in this frame, in its string representation as produced by <c>deparse()</c>.
        /// </summary>
        /// <remarks>
        /// Corresponds to <c>sys.call()</c> for the <see cref="Index"/> of the frame.
        /// </remarks>
        string Call { get; }

        /// <summary>
        /// Whether this frame's environment is <c>.GlobalEnv</c>.
        /// </summary>
        bool IsGlobal { get; }
    }

    public static class RStackFrameExtensions {

        /// <summary>
        /// Like <see cref="RSessionExtensions.DescribeChildrenAsync"/>, but returns children of this environment.
        /// </summary>
        /// <remarks>
        /// If this method is called on a stale frame (i.e if call stack has changed since the <see cref="RSessionExtensions.TracebackAsync"/>
        /// call that produced this frame), the result is undefined - the method can throw <see cref="RException"/>, or produce meaningless
        /// output.
        /// </remarks>
        public static Task<IReadOnlyList<IREvaluationResultInfo>> DescribeChildrenAsync(
            this IRStackFrame frame,
            REvaluationResultProperties properties,
            string repr,
            int? maxCount = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            frame.Session.DescribeChildrenAsync(frame.EnvironmentExpression, "base::environment()", properties, repr, maxCount, cancellationToken);

        /// <summary>
        /// Produces an object describing this frame's environment. <see cref="REvaluationResultInfoExtensions.DescribeChildrenAsync"/>
        /// method can then be used to retrieve the variables in the frame.
        /// </summary>
        /// <param name="properties">
        /// Which properties of the returned object should be provided. Note that it should include at least the following flags
        /// argument value in order for <see cref="REvaluationResultInfoExtensions.DescribeChildrenAsync"/> to be working. 
        /// </param>
        /// <remarks>
        /// <para>
        /// There is no guarantee that the returned evaluation result is <see cref="IRValueInfo"/>. Retrieving the frame environment involves
        /// evaluating <see cref="EnvironmentExpression"/>, and like any evaluation, it can fail. Caller should check for <see cref="IRErrorInfo"/>
        /// and handle it accordingly. However, it is never <see cref="IRPromiseInfo"/> or <see cref="IRActiveBindingInfo"/>.
        /// </para>
        /// <para>
        /// If this method is called on a stale frame (i.e if call stack has changed since the <see cref="RSessionExtensions.TracebackAsync"/>
        /// call that produced this frame), the result is undefined, and can be an error result, or contain unrelated data.
        /// </para>
        /// </remarks>
        public static Task<IRValueInfo> DescribeEnvironmentAsync(
            this IRStackFrame frame,
            REvaluationResultProperties properties,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            properties |= ExpressionProperty | LengthProperty | AttributeCountProperty | FlagsProperty;
            return frame.EvaluateAndDescribeAsync("base::environment()", properties, null, cancellationToken);
        }

        /// <summary>
        /// Same as <see cref="DescribeEnvironmentAsync(IRStackFrame, REvaluationResultProperties, CancellationToken)"/>,
        /// but the only <c>properties</c> that are fetched are those that are necessary to invoke
        /// <see cref="REvaluationResultInfoExtensions.DescribeChildrenAsync"/> on the returned value.
        /// </summary>
        public static Task<IRValueInfo> DescribeEnvironmentAsync(this IRStackFrame frame, CancellationToken cancellationToken = default(CancellationToken)) =>
            frame.DescribeEnvironmentAsync(REvaluationResultProperties.None, cancellationToken);

        /// <summary>
        /// Same as <see cref="Microsoft.R.DataInspection.RSessionExtensions.TryEvaluateAndDescribeAsync(string, string, string, REvaluationResultProperties, int?, CancellationToken)"/>,
        /// but passes <see cref="EnvironmentExpression"/> of this frame as <c>environmentExpression</c> argument,
        /// </summary>
        public static Task<IREvaluationResultInfo> TryEvaluateAndDescribeAsync(
            this IRStackFrame frame,
            string expression,
            string name,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            frame.Session.TryEvaluateAndDescribeAsync(frame.EnvironmentExpression, expression, name, properties, repr, cancellationToken);

        /// <summary>
        /// Same as <see cref="TryEvaluateAndDescribeAsync(IRStackFrame, string, string, REvaluationResultProperties, string, CancellationToken)"/>,
        /// but uses <see langword="null"/> for <c>name</c>.
        /// </summary>
        public static Task<IREvaluationResultInfo> TryEvaluateAndDescribeAsync(
            this IRStackFrame frame,
            string expression,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            frame.TryEvaluateAndDescribeAsync(expression, null, properties, repr, cancellationToken);

        /// <summary>
        /// Same as <see cref="Microsoft.R.DataInspection.RSessionExtensions.EvaluateAndDescribeAsync(string, string, string, REvaluationResultProperties, int?, CancellationToken)"/>,
        /// but passes <see cref="EnvironmentExpression"/> of this frame as <c>environmentExpression</c> argument,
        /// </summary>
        public static Task<IRValueInfo> EvaluateAndDescribeAsync(
            this IRStackFrame frame,
            string expression,
            string name,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            frame.Session.EvaluateAndDescribeAsync(frame.EnvironmentExpression, expression, name, properties, repr, cancellationToken);

        /// <summary>
        /// Same as <see cref="EvaluateAndDescribeAsync(IRStackFrame, string, string, REvaluationResultProperties, string, CancellationToken)"/>,
        /// but uses <see langword="null"/> for <c>name</c>.
        /// </summary>
        public static Task<IRValueInfo> EvaluateAndDescribeAsync(
            this IRStackFrame frame,
            string expression,
            REvaluationResultProperties properties,
            string repr,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            frame.EvaluateAndDescribeAsync(expression, null, properties, repr, cancellationToken);
    }
}
