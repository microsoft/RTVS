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
        /// Indicates that the result of this evaluation is a value that should be serialized to JSON.
        /// When this flag is set, <see cref="REvaluationResult.StringResult"/> will be null, but
        /// <see cref="REvaluationResult.JsonResult"/> will contain the value after it has been deserialized.
        /// </summary>
        Json = 1 << 2,
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
    }
}

