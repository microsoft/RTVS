// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.ExecutionTracing {
    /// <summary>
    /// A breakpoint in R code.
    /// </summary>
    public interface IRBreakpoint {
        IRExecutionTracer Tracer { get; }

        RSourceLocation Location { get; }

        event EventHandler BreakpointHit;

        Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
