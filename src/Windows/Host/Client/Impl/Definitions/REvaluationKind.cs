// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    [Flags]
    public enum REvaluationKind {
        Normal = 0,
        /// <summary>
        /// Allows other evaluations to occur while this evaluation is ongoing.
        /// </summary>
        Reentrant = 1 << 1,
        /// <summary>
        /// Indicates that this expression should be evaluated in the base environment (<c>baseenv()</c>).
        /// Not compatible with <see cref="EmptyEnv"/>.
        /// </summary>
        BaseEnv = 1 << 3,
        /// <summary>
        /// Indicates that this expression should be evaluated in the empty environment (<c>emptyenv()</c>).
        /// Not compatible with <see cref="BaseEnv"/>.
        /// </summary>
        EmptyEnv = 1 << 4,
        /// <summary>
        /// Allows this expression to be cancelled.
        /// </summary>
        /// <remarks>
        /// If this evaluation happens as a nested evaluation inside some other <seealso cref="Reentrant"/>
        /// evaluation, it must be <seealso cref="Cancelable"/> for the outer evaluation to be cancelable.
        /// </remarks>
        Cancelable = 1 << 5,
        /// <summary>
        /// Indicates that this expression can potentially change the observable R state (variable values etc).
        /// </summary>
        /// <remarks>
        /// <see cref="IRSession.Mutated"/> is raised after the evaluation completes.
        /// </remarks>
        Mutating = 1 << 7,
        /// <summary>
        /// Do not retrieve the result of this expression. The returned <see cref="REvaluationResult"/> will
        /// only be used for error reporting, and both <see cref="REvaluationResult.StringResult"/> and
        /// <see cref="REvaluationResult.Result"/> will be <see langword="null"/> upon successful evaluation.
        /// </summary>
        /// <remarks>
        /// Used when expression is evaluated solely for its side effects.
        /// </remarks>
        /// <seealso cref="RExpressionEvaluatorExtensions.ExecuteAsync"/>
        NoResult = 1 << 8,
        /// <summary>
        /// Indicates that the result of the expression should be transmitted as raw bytes,
        /// instead of trying to serialize it as JSON.
        /// </summary>
        /// <remarks>
        /// Expression must return a <c>RAWSXP</c> value or <c>NULL</c>. <c>NULL</c> is considered equivalent to a zero-length <c>RAWSXP</c>.
        /// </remarks>
        RawResult = 1 << 9,
    }
}

