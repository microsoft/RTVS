// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Host.Client;

namespace Microsoft.R.ExecutionTracing {
    /// <seealso cref="IRExecutionTracer.Browse"/>
    public class RBrowseEventArgs : EventArgs {
        /// <summary>
        /// R context for the Browse> prompt.
        /// </summary>
        public IRSessionContext Context { get; }

        /// <summary>
        /// Whether this Browse> prompt signifies the completion of a stepping operation.
        /// </summary>
        public bool HasStepCompleted { get; }
        
        /// <summary>
        /// Breakpoints that were hit at this Browse> prompt.
        /// </summary>
        public IReadOnlyCollection<IRBreakpoint> BreakpointsHit { get; }

        public RBrowseEventArgs(IRSessionContext context, bool isStepCompleted, IReadOnlyCollection<IRBreakpoint> breakpointsHit) {
            Context = context;
            HasStepCompleted = isStepCompleted;
            BreakpointsHit = breakpointsHit ?? new IRBreakpoint[0];
        }
    }
}
