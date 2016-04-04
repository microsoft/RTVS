// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    [Flags]
    public enum DebugEvaluationResultFields : ulong {
        None,
        Expression = 1 << 1,
        Kind = 1 << 2,
        Repr = 1 << 3,
        ReprDeparse = Repr | (1 << 4),
        ReprToString = Repr | (1 << 5),
        ReprStr = Repr | (1 << 6),
        TypeName = 1 << 7,
        Classes = 1 << 8,
        Length = 1 << 9,
        SlotCount = 1 << 10,
        AttrCount = 1 << 11,
        NameCount = 1 << 12,
        Dim = 1 << 13,
        EnvName = 1 << 14,
        Flags = 1 << 15,
    }

    internal static class DebugEvaluationResultFieldsExtensions {
        private static readonly Dictionary<DebugEvaluationResultFields, string> _mapping = new Dictionary<DebugEvaluationResultFields, string> {
            [DebugEvaluationResultFields.Expression] = "expression",
            [DebugEvaluationResultFields.Kind] = "kind",
            [DebugEvaluationResultFields.Repr] = "repr",
            [DebugEvaluationResultFields.ReprDeparse] = "repr.deparse",
            [DebugEvaluationResultFields.ReprToString] = "repr.toString",
            [DebugEvaluationResultFields.ReprStr] = "repr.str",
            [DebugEvaluationResultFields.TypeName] = "type",
            [DebugEvaluationResultFields.Classes] = "classes",
            [DebugEvaluationResultFields.Length] = "length",
            [DebugEvaluationResultFields.SlotCount] = "slot_count",
            [DebugEvaluationResultFields.AttrCount] = "attr_count",
            [DebugEvaluationResultFields.NameCount] = "name_count",
            [DebugEvaluationResultFields.Dim] = "dim",
            [DebugEvaluationResultFields.EnvName] = "env_name",
            [DebugEvaluationResultFields.Flags] = "flags",
        };

        public static string ToRVector(this DebugEvaluationResultFields fields) {
            var fieldNames = _mapping.Where(kv => fields.HasFlag(kv.Key)).Select(kv => "'" + kv.Value + "'");
            return Invariant($"base::c({string.Join(", ", fieldNames)})");
        }
    }

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

            return new DebugValueEvaluationResult(stackFrame, expression, name, json);
        }

        public Task<DebugEvaluationResult> SetValueAsync(string value, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrEmpty(Expression)) {
                throw new InvalidOperationException(Invariant($"{nameof(SetValueAsync)} is not supported for this {nameof(DebugEvaluationResult)} because it doesn't have an associated {nameof(Expression)}."));
            }

            return StackFrame.EvaluateAsync(Invariant($"{Expression} <- {value}"), DebugEvaluationResultFields.None, reprMaxLength: 0, cancellationToken: cancellationToken);
        }

        public Task<DebugEvaluationResult> EvaluateAsync(
            DebugEvaluationResultFields fields,
            int? reprMaxLength = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            if (StackFrame == null) {
                throw new InvalidOperationException("Cannot re-evaluate an evaluation result that is not tied to a frame.");
            }
            if (Expression == null) {
                throw new InvalidOperationException("Cannot re-evaluate an evaluation result that does not have an associated expression.");
            }

            return StackFrame.EvaluateAsync(Expression, Name, fields, reprMaxLength, cancellationToken);
        }

        public override bool Equals(object obj) =>
            base.Equals(obj) || (obj as IEquatable<DebugEvaluationResult>)?.Equals(this) == true;

        public override int GetHashCode() =>
            base.GetHashCode();
    }
}
