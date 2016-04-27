// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// Used to specify properties of <see cref="DebugValueEvaluationResult"/> to fill when evaluating expressions using
    /// <see cref="DebugSession.EvaluateAsync"/>, <see cref="DebugStackFrame.EvaluateAsync"/>, or
    /// <see cref="DebugValueEvaluationResult.GetChildrenAsync"/>.
    /// </summary>
    [Flags]
    public enum DebugEvaluationResultFields : ulong {
        None,
        Expression = 1 << 1,
        Kind = 1 << 2,
        TypeName = 1 << 7,
        Classes = 1 << 8,
        Length = 1 << 9,
        SlotCount = 1 << 10,
        AttrCount = 1 << 11,
        NameCount = 1 << 12,
        Dim = 1 << 13,
        EnvName = 1 << 14,
        Flags = 1 << 15,
        Children = Expression | Length | AttrCount | SlotCount | NameCount | Flags,
    }

    internal static class DebugEvaluationResultFieldsExtensions {
        private static readonly Dictionary<DebugEvaluationResultFields, string> _mapping = new Dictionary<DebugEvaluationResultFields, string> {
            [DebugEvaluationResultFields.Expression] = "expression",
            [DebugEvaluationResultFields.Kind] = "kind",
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
}
