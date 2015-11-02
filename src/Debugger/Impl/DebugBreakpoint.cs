using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
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
            return (FileName?.GetHashCode() ?? 0) ^ LineNumber.GetHashCode();
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
        internal int UseCount { get; set; }

        public event EventHandler BreakpointHit;

        internal DebugBreakpoint(DebugSession session, DebugBreakpointLocation location) {
            Session = session;
            Location = location;
        }

        internal async Task SetBreakpointAsync() {
            TaskUtilities.AssertIsOnBackgroundThread();

            // Tracer expression must be in sync with DebugStackFrame._breakpointRegex
            var location = Invariant($"{Location.FileName.ToRStringLiteral()}, {Location.LineNumber}");
            var tracer = Invariant($"quote({{rtvs:::breakpoint({location})}})");
            var res = await Session.EvaluateAsync(Invariant($"setBreakpoint({location}, tracer={tracer})"));
            if (res is DebugErrorEvaluationResult) {
                throw new InvalidOperationException(Invariant($"{res.Expression}: {res}"));
            }
            ++UseCount;
        }

        public async Task DeleteAsync() {
            Trace.Assert(UseCount > 0);
            await TaskUtilities.SwitchToBackgroundThread();

            if (--UseCount == 0) {
                var res = await Session.EvaluateAsync(Invariant($"setBreakpoint({Location.FileName.ToRStringLiteral()}, {Location.LineNumber}, clear=TRUE)"));
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
