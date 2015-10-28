using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

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

        internal static DebugEvaluationResult Parse(DebugStackFrame stackFrame, string name, JObject json) {
            var expression = json.Value<string>("expression");

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

            if (json["value"] != null) {
                return new DebugValueEvaluationResult(stackFrame, expression, name, json);
            }

            throw new InvalidDataException(Invariant($"Could not determine kind of evaluation result: {json}"));
        }

        public Task<DebugEvaluationResult> SetValueAsync(string value) {
            return StackFrame.EvaluateAsync(Invariant($"{Expression} <- {value}"));
        }
    }

    public class DebugErrorEvaluationResult : DebugEvaluationResult {
        public string ErrorText { get; }

        public DebugErrorEvaluationResult(DebugStackFrame stackFrame, string expression, string name, string errorText)
            : base(stackFrame, expression, name) {
            ErrorText = errorText;
        }

        public override string ToString() {
            return Invariant($"ERROR: {ErrorText}");
        }
    }

    public enum DebugValueEvaluationResultKind {
        UnnamedItem,
        NamedItem,
        Slot,
    }

    public enum DebugValueEvaluationResultFlags {
        Atomic = 1 << 1,
        Recursive = 1 << 2,
        HasParentEnvironment = 1 << 3,
    }

    public class DebugValueEvaluationResult : DebugEvaluationResult {
        public DebugValueEvaluationResultKind Kind { get; }
        public string Value { get; }
        public string RawValue { get; }
        public string TypeName { get; }
        public IReadOnlyList<string> Classes { get; }
        public int Length { get; }
        public int AttributeCount { get; }
        public int SlotCount { get; }
        public DebugValueEvaluationResultFlags Flags { get; }
        public string Str { get; }

        public bool IsAtomic => Flags.HasFlag(DebugValueEvaluationResultFlags.Atomic);
        public bool IsRecursive => Flags.HasFlag(DebugValueEvaluationResultFlags.Recursive);
        public bool HasAttributes => AttributeCount != 0;
        public bool HasSlots => SlotCount != 0;
        public bool HasChildren => HasSlots || Length > (IsAtomic ? 1 : 0);

        internal DebugValueEvaluationResult(DebugStackFrame stackFrame, string expression, string name, JObject json)
            : base(stackFrame, expression, name) {

            Value = json.Value<string>("value");
            RawValue = json.Value<string>("raw_value");
            TypeName = json.Value<string>("type");
            Classes = json.Value<JArray>("classes").Select(t => t.Value<string>()).ToArray();
            Length = json.Value<int>("length");
            AttributeCount = json.Value<int>("attr_count");
            SlotCount = json.Value<int>("slot_count");

            var kind = json.Value<string>("kind");
            switch (kind) {
                case null:
                case "[[":
                    Kind = DebugValueEvaluationResultKind.UnnamedItem;
                    break;
                case "$":
                    Kind = DebugValueEvaluationResultKind.NamedItem;
                    break;
                case "@":
                    Kind = DebugValueEvaluationResultKind.Slot;
                    break;
                default:
                    throw new InvalidDataException(Invariant($"Invalid kind '{kind}' in:\n\n{json}"));
            }

            var flags = json.Value<JArray>("flags").Select(v => v.Value<string>());
            foreach (var flag in flags) {
                switch (flag) {
                    case "atomic":
                        Flags |= DebugValueEvaluationResultFlags.Atomic;
                        break;
                    case "recursive":
                        Flags |= DebugValueEvaluationResultFlags.Recursive;
                        break;
                    case "has_parent_env":
                        Flags |= DebugValueEvaluationResultFlags.HasParentEnvironment;
                        break;
                    default:
                        throw new InvalidDataException(Invariant($"Unrecognized flag '{flag}' in:\n\n{json}"));
                }
            }

            Str = json.Value<string>("str");
        }

        public async Task<IReadOnlyList<DebugEvaluationResult>> GetChildrenAsync(bool useStr = false, int? truncateLength = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (StackFrame == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that is not tied to a frame.");
            }

            string parameter = Invariant($"{Expression.ToRStringLiteral()}, {StackFrame.SysFrame}, use.str={useStr.ToString().ToUpperInvariant()}");
            if (truncateLength.HasValue) {
                parameter += Invariant($", truncate.length={truncateLength.Value}");
            } else {
                parameter += Invariant($", truncate.length=NULL");
            }

            var call = Invariant($".rtvs.children({parameter})");
            var jChildren = await StackFrame.Session.InvokeDebugHelperAsync<JArray>(call);
            Trace.Assert(
                jChildren.Children().All(t => t is JObject),
                Invariant($".rtvs.children(): object of objects expected.\n\n{jChildren}"));

            var children = new List<DebugEvaluationResult>();
            foreach (var child in jChildren) {
                var childObject = (JObject)child;
                Trace.Assert(
                    childObject.Count == 1,
                    Invariant($".rtvs.children(): each object is expected contain one object\n\n"));
                foreach (var kv in childObject) {
                    var name = kv.Key;
                    var jEvalResult = (JObject)kv.Value;
                    var evalResult = Parse(StackFrame, name, jEvalResult);
                    children.Add(evalResult);
                }
            }

            return children.ToArray();
        }

        public override string ToString() {
            return Invariant($"VALUE: {TypeName} {Value}");
        }
    }

    public class DebugPromiseEvaluationResult : DebugEvaluationResult {
        public string Code { get; }

        public DebugPromiseEvaluationResult(DebugStackFrame stackFrame, string expression, string name, string code)
            : base(stackFrame, expression, name) {
            Code = code;
        }

        public override string ToString() {
            return Invariant($"PROMISE: {Code}");
        }
    }

    public class DebugActiveBindingEvaluationResult : DebugEvaluationResult {
        public DebugActiveBindingEvaluationResult(DebugStackFrame stackFrame, string expression, string name)
            : base(stackFrame, expression, name) {
        }
    }
}
