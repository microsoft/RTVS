using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    internal enum DebugStackFrameKind {
        Normal,
        DoTrace, // .doTrace(if (rtvs:::is_breakpoint(...)) browser())
    }

    public class DebugStackFrame {
        private static readonly Regex _doTraceRegex = new Regex(
            @"^\.doTrace\(.*rtvs:::is_breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\).*\)$",
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

        public Task<DebugEvaluationResult> EvaluateAsync(
            string expression,
            string name = null,
            DebugEvaluationResultFields fields = DebugEvaluationResultFields.All,
            int? reprMaxLength = null
        ) {
            return Session.EvaluateAsync(this, expression, name, null, fields, reprMaxLength);
        }

        public Task<DebugEvaluationResult> GetEnvironmentAsync(
            DebugEvaluationResultFields fields = DebugEvaluationResultFields.Expression | DebugEvaluationResultFields.Length | DebugEvaluationResultFields.AttrCount
        ) {
            return EvaluateAsync("base::environment()", fields: fields);
        }
    }
}
