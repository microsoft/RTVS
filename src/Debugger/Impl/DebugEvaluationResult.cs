using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    public abstract class DebugEvaluationResult {
        public string Expression { get; }

        internal DebugEvaluationResult(string expression) {
            Expression = expression;
        }
    }

    public class DebugFailedEvaluationResult : DebugEvaluationResult {
        public string ErrorText { get; }

        public DebugFailedEvaluationResult(string expression, string errorText)
            : base(expression) {
            ErrorText = errorText;
        }

        public override string ToString() {
            return $"{nameof(DebugFailedEvaluationResult)}: {ErrorText}";
        }
    }

    public class DebugSuccessfulEvaluationResult : DebugEvaluationResult {
        public DebugStackFrame StackFrame { get; }
        public string Value { get; }
        public string RawValue { get; }
        public string TypeName { get; }
        public bool HasChildren { get; }

        public DebugSuccessfulEvaluationResult(DebugStackFrame stackFrame, string expression, string value, string rawValue, string typeName)
            : base(expression) {
            StackFrame = stackFrame;
            Value = value;
            RawValue = rawValue;
            TypeName = typeName;
        }

        public DebugSuccessfulEvaluationResult(DebugStackFrame stackFrame, string expression, JObject json)
            : this(stackFrame, expression, (string)json["value"], (string)json["raw_value"], (string)json["type"]) {
        }

        public Task<DebugEvaluationResult> SetValueAsync(string value) {
            return StackFrame.EvaluateAsync($"{Expression} <- {value}");
        }

        public Task<DebugSuccessfulEvaluationResult[]> GetChildrenAsync() {
            if (StackFrame == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that is not tied to a frame.");
            }

            // TODO
            return Task.FromResult(new DebugSuccessfulEvaluationResult[0]);
        }

        public override string ToString() {
            return $"{nameof(DebugSuccessfulEvaluationResult)}: {TypeName} {Value}";
        }
    }
}
