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
    public struct DebugBreakpointLocation : IEquatable<DebugBreakpointLocation> {
        public string FileName { get; }
        public int LineNumber { get; }

        public DebugBreakpointLocation(string fileName, int lineNumber) {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        public override int GetHashCode() {
            return new { FileName, LineNumber }.GetHashCode();
        }

        public override bool Equals(object obj) {
            return (obj as DebugBreakpointLocation?)?.Equals(this) ?? false;
        }

        public bool Equals(DebugBreakpointLocation other) {
            return FileName == other.FileName && LineNumber == other.LineNumber;
        }

        public override string ToString() {
            return Invariant($"{FileName}:{LineNumber}");
        }
    }

    public sealed class DebugBreakpoint {
        public DebugSession Session { get; }
        public DebugBreakpointLocation Location { get; }
        internal int UseCount { get; private set; }

        public event EventHandler BreakpointHit;

        internal DebugBreakpoint(DebugSession session, DebugBreakpointLocation location) {
            Session = session;
            Location = location;
        }

        internal string GetAddBreakpointExpression(bool reapply) {
            string fileName = null;
            try {
                fileName = Path.GetFileName(Location.FileName);
            } catch (ArgumentException) {
                return null;
            }

            return Invariant($"rtvs:::add_breakpoint({fileName.ToRStringLiteral()}, {Location.LineNumber}, {(reapply ? "TRUE" : "FALSE")})");
        }

        internal async Task SetBreakpointAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();
            await Session.InvokeDebugHelperAsync(GetAddBreakpointExpression(true), cancellationToken);
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

                var code = Invariant($"rtvs:::remove_breakpoint({fileName.ToRStringLiteral()}, {Location.LineNumber})");
                var res = await Session.EvaluateAsync(code, DebugEvaluationResultFields.None, cancellationToken);
                if (res is DebugErrorEvaluationResult) {
                    throw new InvalidOperationException(Invariant($"{res.Expression}: {res}"));
                }
            }
        }

        internal void RaiseBreakpointHit() {
            BreakpointHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
