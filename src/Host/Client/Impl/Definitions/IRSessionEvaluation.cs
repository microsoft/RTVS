// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionEvaluation : IDisposable {
        IReadOnlyList<IRContext> Contexts { get; }
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal);
    }
}