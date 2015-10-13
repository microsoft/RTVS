using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    public class DebugStackFrame {
        private static readonly Regex _doTraceRegex = new Regex(
            @"^\.doTrace\(\.rtvs\.breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\)\)$",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        private static readonly Regex _breakpointRegex = new Regex(
            @"^\.rtvs\.breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\)$",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        private enum FrameKind {
            Normal,
            DoTrace, // .doTrace(.rtvs.breakpoint(...))
            DoTraceInternals, // everything between the one above and the one below
            Breakpoint, // .rtvs.breakpoint(...)
            TracebackAfterBreakpoint // .rtvs.traceback() immediately following .rtvs.breakpoint(...)
        }

        public DebugSession Session { get; }

        public int Index { get; }

        public DebugStackFrame CallingFrame { get; }

        public string FileName { get; }

        public int? LineNumber { get; }

        public string Call { get; }

        public bool IsGlobal { get; }

        private readonly FrameKind _frameKind;

        public bool IsDebuggerInternal =>
            _frameKind == FrameKind.DoTraceInternals ||
            _frameKind == FrameKind.Breakpoint ||
            _frameKind == FrameKind.TracebackAfterBreakpoint;

        internal DebugStackFrame(DebugSession session, int index, DebugStackFrame callingFrame, JObject jFrame) {
            Session = session;
            Index = index;
            CallingFrame = callingFrame;

            FileName = (string)jFrame["filename"];
            LineNumber = (int?)(double?)jFrame["line_number"];
            Call = (string)jFrame["call"];
            IsGlobal = (bool)jFrame["is_global"];

            var match = _doTraceRegex.Match(Call);
            if (match.Success) {
                _frameKind = FrameKind.DoTrace;
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
                    Debug.Fail($"Couldn't parse RTVS .doTrace call: {Call}");
                }
            } else if (_breakpointRegex.IsMatch(Call)) {
                _frameKind = FrameKind.Breakpoint;
            } else {
                switch (CallingFrame?._frameKind) {
                    case FrameKind.DoTrace:
                    case FrameKind.DoTraceInternals:
                        _frameKind = FrameKind.DoTraceInternals;
                        break;
                    case FrameKind.Breakpoint:
                        if (Call == ".rtvs.traceback()") {
                            _frameKind = FrameKind.TracebackAfterBreakpoint;
                        }
                        break;
                }
            }
        }

        public Task<DebugEvaluationResult> EvaluateAsync(string expression) {
            return Session.EvaluateAsync(this, expression, $"sys.frame({Index})");
        }

        public async Task<IReadOnlyDictionary<string, DebugEvaluationResult>> GetVariablesAsync() {
            var vars = new Dictionary<string, DebugEvaluationResult>();
            var res = await Session.EvaluateRawAsync($".rtvs.env_vars(sys.frame({Index}))").ConfigureAwait(false);
            var jFrameVars = JObject.Parse(res.Result);
            foreach (var kv in jFrameVars) {
                var name = kv.Key;
                var jEvalResult = (JObject)kv.Value;
                vars[name] = DebugEvaluationResult.Parse(this, name, jEvalResult);
            }

            return vars;
        }
    }
}
