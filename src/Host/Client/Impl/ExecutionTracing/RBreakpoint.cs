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

namespace Microsoft.R.ExecutionTracing {
    internal sealed class RBreakpoint : IRBreakpoint {
        private readonly RExecutionTracer _tracer;

        public IRExecutionTracer Tracer => _tracer;

        public RSourceLocation Location { get; }

        internal int UseCount { get; private set; }

        public event EventHandler BreakpointHit;

        internal RBreakpoint(RExecutionTracer tracer, RSourceLocation location) {
            _tracer = tracer;
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
            await Tracer.Session.ExecuteAsync(GetAddBreakpointExpression(false), cancellationToken);
            // TODO: mark breakpoint as invalid if this fails.
        }

        internal async Task SetBreakpointAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();
            await Tracer.Session.ExecuteAsync(GetAddBreakpointExpression(true), cancellationToken);
            ++UseCount;
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            Trace.Assert(UseCount > 0);
            await TaskUtilities.SwitchToBackgroundThread();
            await _tracer.InitializeAsync(cancellationToken);

            string fileName = null;
            try {
                fileName = Path.GetFileName(Location.FileName);
            } catch (ArgumentException) {
                return;
            }

            if (--UseCount == 0) {
                _tracer.RemoveBreakpoint(this);

                var code = $"rtvs:::remove_breakpoint({fileName.ToRStringLiteral()}, {Location.LineNumber})";
                try {
                    await Tracer.Session.ExecuteAsync(code, cancellationToken);
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
