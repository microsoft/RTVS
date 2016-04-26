// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger {
    /// <seealso cref="DebugSession.Browse"/>
    public class DebugBrowseEventArgs : EventArgs {
        /// <summary>
        /// R context for the Browse> prompt.
        /// </summary>
        public IRSessionContext Context { get; }
        /// <summary>
        /// Whether this Browse> prompt signifies the completion of a stepping operation.
        /// </summary>
        public bool IsStepCompleted { get; }
        /// <summary>
        /// Breakpoints that were hit at this Browse> prompt.
        /// </summary>
        public IReadOnlyCollection<DebugBreakpoint> BreakpointsHit { get; }

        public DebugBrowseEventArgs(IRSessionContext context, bool isStepCompleted, IReadOnlyCollection<DebugBreakpoint> breakpointsHit) {
            Context = context;
            IsStepCompleted = isStepCompleted;
            BreakpointsHit = breakpointsHit ?? new DebugBreakpoint[0];
        }
    }
}
