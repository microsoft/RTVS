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
        public int Index { get; }

        public DebugSession Session { get; }

        public string FileName { get; }

        public int? LineNumber { get; }

        public string CallingExpression { get; }

        public bool IsGlobal { get; }

        internal DebugStackFrame(DebugSession session, int index, string fileName, int? lineNumber, string callingExpression, bool isGlobal) {
            Debug.Assert(index >= 0);
            Session = session;
            Index = index;
            FileName = fileName;
            LineNumber = lineNumber;
            CallingExpression = callingExpression;
            IsGlobal = isGlobal;
        }

        internal DebugStackFrame(DebugSession session, int index, JObject jFrame)
            : this(session, index, (string)jFrame["filename"], (int?)(double?)jFrame["linenum"], (string)jFrame["call"], (bool)jFrame["is_global"]) {
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
                vars[name] = new DebugSuccessfulEvaluationResult(this, name, jEvalResult);
            }

            return vars;
        }
    }
}
