// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    public enum DebugValueEvaluationResultKind {
        UnnamedItem,
        NamedItem,
        Slot,
    }

    [Flags]
    public enum DebugValueEvaluationResultFlags {
        None,
        Atomic = 1 << 1,
        Recursive = 1 << 2,
        HasParentEnvironment = 1 << 3,
    }

    public class DebugValueEvaluationResult : DebugEvaluationResult {
        public DebugValueEvaluationResultKind Kind { get; }
        public string TypeName { get; }
        public IReadOnlyList<string> Classes { get; }
        public int? Length { get; }
        public int? AttributeCount { get; }
        public int? SlotCount { get; }
        public int? NameCount { get; }
        public IReadOnlyList<int> Dim { get; }
        public DebugValueEvaluationResultFlags Flags { get; }

        public bool IsAtomic => Flags.HasFlag(DebugValueEvaluationResultFlags.Atomic);
        public bool IsRecursive => Flags.HasFlag(DebugValueEvaluationResultFlags.Recursive);
        public bool HasAttributes => AttributeCount != null && AttributeCount != 0;
        public bool HasSlots => SlotCount != null && SlotCount != 0;
        public bool HasChildren => HasSlots || (Length != null && (Length > (IsAtomic || (TypeName == "closure" || TypeName == "symbol") ? 1 : 0)));

        private JObject _reprObj;

        internal DebugValueEvaluationResult(DebugStackFrame stackFrame, string expression, string name, JObject json)
            : base(stackFrame, expression, name) {

            var repr = json["repr"];
            if (repr != null) {
                _reprObj = repr as JObject;
                if (_reprObj == null) {
                    throw new InvalidDataException(Invariant($"'repr' must be an object in:\n\n{json}"));
                }
            }

            TypeName = json.Value<string>("type");
            Length = json.Value<int?>("length");
            AttributeCount = json.Value<int?>("attr_count");
            SlotCount = json.Value<int?>("slot_count");
            NameCount = json.Value<int?>("name_count");

            var classes = json.Value<JArray>("classes");
            if (classes != null) {
                Classes = classes.Select(t => t.Value<string>()).ToArray();
            }

            var dim = json.Value<JArray>("dim");
            if (dim != null) {
                Dim = dim.Select(t => t.Value<int>()).ToArray();
            }

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

            var flags = json.Value<JArray>("flags")?.Select(v => v.Value<string>());
            if (flags != null) {
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
            }
        }

        public DebugValueRepresentation GetRepresentation(DebugValueRepresentationKind kind = DebugValueRepresentationKind.Normal) {
            if (_reprObj == null) {
                throw new InvalidOperationException("Evaluation result does not have an associated representation.");
            }
            return new DebugValueRepresentation(_reprObj, kind);
        }

        public async Task<IReadOnlyList<DebugEvaluationResult>> GetChildrenAsync(
            DebugEvaluationResultFields fields,
            int? maxLength = null,
            int? reprMaxLength = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (StackFrame == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that is not tied to a frame.");
            }
            if (Expression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated expression.");
            }

            var call = Invariant($@"rtvs:::describe_children({Expression.ToRStringLiteral()}, {StackFrame.EnvironmentExpression}, {fields.ToRVector()}, {maxLength}, {reprMaxLength})");
            var jChildren = await StackFrame.Session.InvokeDebugHelperAsync<JArray>(call, cancellationToken);
            Trace.Assert(
                jChildren.Children().All(t => t is JObject),
                Invariant($"rtvs:::describe_children(): object of objects expected.\n\n{jChildren}"));

            var children = new List<DebugEvaluationResult>();
            foreach (var child in jChildren) {
                var childObject = (JObject)child;
                Trace.Assert(
                    childObject.Count == 1,
                    Invariant($"rtvs:::describe_children(): each object is expected contain one object\n\n"));
                foreach (var kv in childObject) {
                    var name = kv.Key;
                    var jEvalResult = (JObject)kv.Value;
                    var evalResult = Parse(StackFrame, name, jEvalResult);
                    children.Add(evalResult);
                }
            }

            return children;
        }

        public override string ToString() {
            return Invariant($"VALUE: {TypeName} {GetRepresentation().Deparse}");
        }
    }
}
