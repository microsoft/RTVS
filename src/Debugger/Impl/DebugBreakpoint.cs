// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// A breakpoint in R code.
    /// </summary>
    public sealed class DebugBreakpoint {
        public DebugSession Session { get; }
        public DebugSourceLocation Location { get; }
        internal int UseCount { get; private set; }

        public event EventHandler BreakpointHit;

        internal DebugBreakpoint(DebugSession session, DebugSourceLocation location) {
            Session = session;
            Location = location;
        }

        private string GetAddBreakpointExpression(bool reapply) {
            string fileName = null;
            try {
                fileName = Path.GetFileName(Location.FileName);
            } catch (ArgumentException) {
                return null;
            }

            return Invariant($"rtvs:::add_breakpoint({fileName.ToRStringLiteral()}, {Location.LineNumber}, {(reapply ? "TRUE" : "FALSE")})");
        }

        internal async Task ReapplyBreakpointAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();
            await Session.RSession.ExecuteAsync(GetAddBreakpointExpression(false), REvaluationKind.Mutating, cancellationToken);
            // TODO: mark breakpoint as invalid if this fails.
        }

        internal async Task SetBreakpointAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();
            await Session.RSession.ExecuteAsync(GetAddBreakpointExpression(true), REvaluationKind.Mutating, cancellationToken);
            ++UseCount;
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            Trace.Assert(UseCount > 0);
            await TaskUtilities.SwitchToBackgroundThread();
            await Session.InitializeAsync(cancellationToken);

            string fileName = null;
            try {
                fileName = Path.GetFileName(Location.FileName);
            } catch (ArgumentException) {
                return;
            }

            if (--UseCount == 0) {
                Session.RemoveBreakpoint(this);

                var code = $"rtvs:::remove_breakpoint({fileName.ToRStringLiteral()}, {Location.LineNumber})";
                try {
                    await Session.RSession.ExecuteAsync(code, REvaluationKind.Mutating, cancellationToken);
                } catch (RException ex) {
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }
        }

        internal void RaiseBreakpointHit() {
            BreakpointHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
