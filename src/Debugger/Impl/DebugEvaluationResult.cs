using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    public abstract class DebugEvaluationResult {
        public DebugStackFrame StackFrame { get; }
        public string Expression { get; }

        internal DebugEvaluationResult(DebugStackFrame stackFrame, string expression) {
            StackFrame = stackFrame;
            Expression = expression;
        }

        internal static DebugEvaluationResult Parse(DebugStackFrame stackFrame, string expression, JObject json) {
            var errorText = (string)json["error"];
            if (errorText != null) {
                return new DebugErrorEvaluationResult(stackFrame, expression, errorText);
            }

            var code = (string)json["promise"];
            if (code != null) {
                return new DebugPromiseEvaluationResult(stackFrame, expression, code);
            }

            var isActiveBinding = (bool?)json["active_binding"];
            if (isActiveBinding == true) {
                return new DebugActiveBindingEvaluationResult(stackFrame, expression);
            }

            var value = (string)json["value"];
            if (value != null) {
                var rawValue = (string)json["raw_value"];
                var typeName = (string)json["type"];
                return new DebugValueEvaluationResult(stackFrame, expression, value, rawValue, typeName);
            }

            throw new InvalidDataException($"Could not determine kind of evaluation result: {json}");
        }

        public Task<DebugEvaluationResult> SetValueAsync(string value) {
            return StackFrame.EvaluateAsync($"{Expression} <- {value}");
        }
    }

    public class DebugErrorEvaluationResult : DebugEvaluationResult {
        public string ErrorText { get; }

        public DebugErrorEvaluationResult(DebugStackFrame stackFrame, string expression, string errorText)
            : base(stackFrame, expression) {
            ErrorText = errorText;
        }

        public override string ToString() {
            return $"ERROR: {ErrorText}";
        }
    }

    public class DebugValueEvaluationResult : DebugEvaluationResult {
        public string Value { get; }
        public string RawValue { get; }
        public string TypeName { get; }
        public bool HasChildren { get; }

        public DebugValueEvaluationResult(DebugStackFrame stackFrame, string expression, string value, string rawValue, string typeName)
            : base(stackFrame, expression) {
            Value = value;
            RawValue = rawValue;
            TypeName = typeName;
        }

        public Task<DebugValueEvaluationResult[]> GetChildrenAsync() {
            if (StackFrame == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that is not tied to a frame.");
            }

            // TODO
            return Task.FromResult(new DebugValueEvaluationResult[0]);
        }

        public override string ToString() {
            return $"VALUE: {TypeName} {Value}";
        }
    }

    public class DebugPromiseEvaluationResult : DebugEvaluationResult {
        public string Code { get; }

        public DebugPromiseEvaluationResult(DebugStackFrame stackFrame, string expression, string code)
            : base(stackFrame, expression) {
            Code = code;
        }

        public override string ToString() {
            return $"PROMISE: {Code}";
        }
    }

    public class DebugActiveBindingEvaluationResult : DebugEvaluationResult {
        public DebugActiveBindingEvaluationResult(DebugStackFrame stackFrame, string expression)
            : base(stackFrame, expression) {
        }
    }
}
