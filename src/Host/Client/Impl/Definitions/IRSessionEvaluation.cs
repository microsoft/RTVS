// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionEvaluation : IDisposable {
        IReadOnlyList<IRContext> Contexts { get; }
        /// <param name="reentrant">
        /// If <c>true</c>, nested evaluations are possible if R transitions to the state allowing evaluation
        /// while evaluating this expression. Otherwise, no nested evaluations are possible.
        /// </param>
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal);
    }
}