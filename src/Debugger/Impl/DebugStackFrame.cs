using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    internal enum DebugStackFrameKind {
        Normal,
        DoTrace, // .doTrace(rtvs:::breakpoint(...))
        DoTraceInternals, // everything between the one above and the one below
        Breakpoint, // rtvs:::breakpoint(...)
        TracebackAfterBreakpoint // rtvs:::describe_traceback() immediately following rtvs:::breakpoint(...)
    }

    public class DebugStackFrame {
        private static readonly Regex _doTraceRegex = new Regex(
            @"^\.doTrace\(\{\s*rtvs:::breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\)\s*\}\)$",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
        private static readonly Regex _breakpointRegex = new Regex(
            @"^rtvs:::breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\)$",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public DebugSession Session { get; }

        public int Index { get; }

        internal string SysFrame => Invariant($"sys.frame({Index})");

        public DebugStackFrame CallingFrame { get; }

        public string FileName { get; }

        public int? LineNumber { get; }

        public string Call { get; }

        public bool IsGlobal { get; }

        internal DebugStackFrameKind FrameKind { get; }

        public bool IsDebuggerInternal =>
            FrameKind == DebugStackFrameKind.DoTraceInternals ||
            FrameKind == DebugStackFrameKind.Breakpoint ||
            FrameKind == DebugStackFrameKind.TracebackAfterBreakpoint;

        internal DebugStackFrame(DebugSession session, int index, DebugStackFrame callingFrame, JObject jFrame, DebugStackFrame fallbackFrame = null) {
            Session = session;
            Index = index;
            CallingFrame = callingFrame;

            FileName = jFrame.Value<string>("filename");
            LineNumber = jFrame.Value<int?>("line_number");
            Call = jFrame.Value<string>("call");
            IsGlobal = jFrame.Value<bool?>("is_global") ?? false;

            var match = _doTraceRegex.Match(Call);
            if (match.Success) {
                FrameKind = DebugStackFrameKind.DoTrace;
                try {
                    // When setBreakpoint injects .doTrace calls, it does not inject source information for them.
                    // Consequently, then such a call is on the stack - i.e. when a breakpoint is hit - there is
                    // no information about which filename and line number we're on in the call object.
                    // To work around that, we use our own special wrapper function for tracer instead of just
                    // plain browser(), and we pass this data as arguments to that function. The function itself
                    // does not actually use them, but they appear as part of the calling expression, and we can
                    // extract them from here.
                    // In case setBreakpoint is changed in the future to correctly adjust the source info for the
                    // function, only make use of the parsed data if the values couldn't be properly obtained. 
                    FileName = FileName ?? match.Groups["filename"].Value.FromRStringLiteral();
                    LineNumber = LineNumber ?? int.Parse(match.Groups["line_number"].Value);
                } catch (FormatException) {
                    // This should never happen with .doTrace calls that we insert, but the user can always manually
                    // insert one. Assert in Debug to detect code changes that break our inserted .doTrace.
                    Debug.Fail(Invariant($"Couldn't parse RTVS .doTrace call: {Call}"));
                }
            } else if (_breakpointRegex.IsMatch(Call)) {
                FrameKind = DebugStackFrameKind.Breakpoint;
            } else {
                switch (CallingFrame?.FrameKind) {
                    case DebugStackFrameKind.DoTrace:
                    case DebugStackFrameKind.DoTraceInternals:
                        FrameKind = DebugStackFrameKind.DoTraceInternals;
                        break;
                    case DebugStackFrameKind.Breakpoint:
                        if (Call == "rtvs:::describe_traceback()") {
                            FrameKind = DebugStackFrameKind.TracebackAfterBreakpoint;
                        }
                        break;
                }
            }

            if (fallbackFrame != null) {
                // If we still don't have the filename and line number, use those from the fallback frame.
                // This happens during breakpoint hit processing after the context is unwound from within
                // .doTrace back to the function that called it - because we no longer have .doTrace call,
                // we don't have the file/line information that came from it. But DebugSession will have
                // stashed it away when it had it, and then pass it as a fallback frame if index matches.
                FileName = FileName ?? fallbackFrame.FileName;
                LineNumber = LineNumber ?? fallbackFrame.LineNumber;
            }
        }

        public Task<DebugEvaluationResult> EvaluateAsync(string expression, string name = null) {
            return Session.EvaluateAsync(this, expression, name, SysFrame);
        }

        public Task<DebugEvaluationResult> GetEnvironmentAsync() {
            return EvaluateAsync("environment()");
        }
    }
}
