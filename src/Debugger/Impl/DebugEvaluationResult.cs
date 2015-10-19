using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    public abstract class DebugEvaluationResult {
        public DebugStackFrame StackFrame { get; }
        public string Expression { get; }
        public string Name { get; }

        internal DebugEvaluationResult(DebugStackFrame stackFrame, string expression, string name) {
            StackFrame = stackFrame;
            Expression = expression;
            Name = name;
        }

        internal static DebugEvaluationResult Parse(DebugStackFrame stackFrame, string expression, string name, JObject json) {
            var errorText = json.Value<string>("error");
            if (errorText != null) {
                return new DebugErrorEvaluationResult(stackFrame, expression, name, errorText);
            }

            var code = json.Value<string>("promise");
            if (code != null) {
                return new DebugPromiseEvaluationResult(stackFrame, expression, name, code);
            }

            var isActiveBinding = json.Value<bool?>("active_binding");
            if (isActiveBinding == true) {
                return new DebugActiveBindingEvaluationResult(stackFrame, expression, name);
            }

            var value = json.Value<string>("value");
            if (value != null) {
                return new DebugValueEvaluationResult(stackFrame, expression, name, json);
            }

            throw new InvalidDataException($"Could not determine kind of evaluation result: {json}");
        }

        public Task<DebugEvaluationResult> SetValueAsync(string value) {
            return StackFrame.EvaluateAsync($"{Expression} <- {value}");
        }
    }

    public class DebugErrorEvaluationResult : DebugEvaluationResult {
        public string ErrorText { get; }

        public DebugErrorEvaluationResult(DebugStackFrame stackFrame, string expression, string name, string errorText)
            : base(stackFrame, expression, name) {
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
        public int Length { get; }
        public bool IsAtomic { get; }
        public bool IsRecursive { get; }
        public bool HasAttributes { get; }
        public bool HasSlots { get; }
        public IReadOnlyList<string> Classes { get; }

        public bool HasChildren => HasSlots || Length > (IsAtomic ? 1 : 0);

        internal DebugValueEvaluationResult(DebugStackFrame stackFrame, string expression, string name, JObject json)
            : base(stackFrame, expression, name) {
            Value = json.Value<string>("value");
            RawValue = json.Value<string>("raw_value");
            TypeName = json.Value<string>("type");
            Length = json.Value<int>("length");
            IsAtomic = json.Value<bool>("is_atomic");
            IsRecursive = json.Value<bool>("is_recursive");
            HasAttributes = json.Value<int>("attr_count") > 0;
            HasSlots = json.Value<int>("slot_count") > 0;
            Classes = json.Value<JArray>("class").Select(t => t.Value<string>()).ToArray();
        }

        public async Task<IReadOnlyList<DebugEvaluationResult>> GetChildrenAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            if (StackFrame == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that is not tied to a frame.");
            }

            var call = $".rtvs.children({Expression.ToRStringLiteral()}, {StackFrame.SysFrame})";
            var jChildren = await StackFrame.Session.InvokeDebugHelperAsync<JObject>(call);
            Trace.Assert(
                jChildren.Values().All(t => t is JObject),
                $".rtvs.children(): object of objects expected.\n\n" + jChildren);

            var children = new List<DebugEvaluationResult>();
            foreach (var kv in jChildren) {
                var name = kv.Key;
                var expr = "(" + Expression + ")" + kv.Key;
                var jEvalResult = (JObject)kv.Value;
                var evalResult = Parse(StackFrame, expr, name, jEvalResult);
                children.Add(evalResult);
            }

            return children.ToArray();
        }

        public override string ToString() {
            return $"VALUE: {TypeName} {Value}";
        }
    }

    public class DebugPromiseEvaluationResult : DebugEvaluationResult {
        public string Code { get; }

        public DebugPromiseEvaluationResult(DebugStackFrame stackFrame, string expression, string name, string code)
            : base(stackFrame, expression, name) {
            Code = code;
        }

        public override string ToString() {
            return $"PROMISE: {Code}";
        }
    }

    public class DebugActiveBindingEvaluationResult : DebugEvaluationResult {
        public DebugActiveBindingEvaluationResult(DebugStackFrame stackFrame, string expression, string name)
            : base(stackFrame, expression, name) {
        }
    }
}
