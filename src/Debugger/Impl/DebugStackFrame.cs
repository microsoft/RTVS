using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    public class DebugStackFrame {
        public DebugSession Session { get; }

        public int Index { get; }

        public DebugStackFrame CallingFrame { get; }

        public string FileName { get; }

        public int? LineNumber { get; }

        public string Call { get; }

        public bool IsGlobal { get; }

        internal DebugStackFrame(DebugSession session, int index, DebugStackFrame callingFrame, string fileName, int? lineNumber, string call, bool isGlobal) {
            Debug.Assert(index >= 0);
            Session = session;
            Index = index;
            CallingFrame = callingFrame;
            FileName = fileName;
            LineNumber = lineNumber;
            Call = call;
            IsGlobal = isGlobal;
        }

        internal static DebugStackFrame Parse(DebugSession session, int index, DebugStackFrame callingFrame, JObject jFrame) {
            var fileName = (string)jFrame["filename"];
            var lineNumber = (int?)(double?)jFrame["line_number"];
            var call = (string)jFrame["call"];
            var isGlobal = (bool)jFrame["is_global"];
            return new DebugStackFrame(session, index, callingFrame, fileName, lineNumber, call, isGlobal);
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
