// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.R.DataInspection;
using Microsoft.R.StackTracing;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    /// <summary>
    /// Used to specify properties of <see cref="IRValueInfo"/> to fill when evaluating expressions using
    /// <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/> and its various wrappers.
    /// </summary>
    [Flags]
    public enum RValueProperties : ulong {
        None,
        Expression = 1 << 1,
        Kind = 1 << 2,
        TypeName = 1 << 3,
        Classes = 1 << 4,
        Length = 1 << 5,
        SlotCount = 1 << 6,
        AttrCount = 1 << 7,
        NameCount = 1 << 8,
        Dim = 1 << 9,
        EnvName = 1 << 10,
        Flags = 1 << 11,
        Children = Expression | Length | AttrCount | SlotCount | NameCount | Flags,
    }

    internal static class RValuePropertiesExtensions {
        private static readonly Dictionary<RValueProperties, string> _mapping = new Dictionary<RValueProperties, string> {
            [RValueProperties.Expression] = "expression",
            [RValueProperties.Kind] = "kind",
            [RValueProperties.TypeName] = "type",
            [RValueProperties.Classes] = "classes",
            [RValueProperties.Length] = "length",
            [RValueProperties.SlotCount] = "slot_count",
            [RValueProperties.AttrCount] = "attr_count",
            [RValueProperties.NameCount] = "name_count",
            [RValueProperties.Dim] = "dim",
            [RValueProperties.EnvName] = "env_name",
            [RValueProperties.Flags] = "flags",
        };

        public static string ToRVector(this RValueProperties properties) {
            var fieldNames = _mapping.Where(kv => properties.HasFlag(kv.Key)).Select(kv => "'" + kv.Value + "'");
            return Invariant($"base::c({string.Join(", ", fieldNames)})");
        }
    }
}
