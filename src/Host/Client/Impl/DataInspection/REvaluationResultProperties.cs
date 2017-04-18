// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;
using static Microsoft.R.Host.Client.REvaluationResult;

namespace Microsoft.R.DataInspection {
    /// <summary>
    /// Used to specify properties of <see cref="IREvaluationResultInfo"/>, <see cref="IRValueInfo"/>,
    /// <see cref="IRPromiseInfo"/> and <see cref="IRActiveBindingInfo"/> to fill when evaluating
    /// expressions using <see cref="RSessionExtensions.TryEvaluateAndDescribeAsync"/> and its various
    /// wrappers.
    /// </summary>
    [Flags]
    public enum REvaluationResultProperties : ulong {
        None,
        /// <seealso cref="IREvaluationResultInfo.Expression"/>
        ExpressionProperty = 1 << 1,
        /// <seealso cref="IRValueInfo.AccessorKind"/>
        AccessorKindProperty = 1 << 2,
        /// <seealso cref="IRValueInfo.TypeName"/>
        TypeNameProperty = 1 << 3,
        /// <seealso cref="IRValueInfo.Classes"/>
        ClassesProperty = 1 << 4,
        /// <seealso cref="IRValueInfo.Length"/>
        LengthProperty = 1 << 5,
        /// <seealso cref="IRValueInfo.SlotCount"/>
        SlotCountProperty = 1 << 6,
        /// <seealso cref="IRValueInfo.AttributeCount"/>
        AttributeCountProperty = 1 << 7,
        /// <seealso cref="IRValueInfo.NameCount"/>
        NameCountProperty = 1 << 8,
        /// <seealso cref="IRValueInfo.Dim"/>
        DimProperty = 1 << 9,
        /// <seealso cref="IRValueInfo.Flags"/>
        FlagsProperty = 1 << 10,
        /// <seealso cref="IRActiveBindingInfo.ComputedValue"/>
        ComputedValueProperty = 1 << 11,
        /// <seealso cref="IRValueInfo.CanCoerceToDataFrame"/>
        CanCoerceToDataFrameProperty = 1 << 12,
        /// <seealso cref="IRValueInfo.HasChildren"/>
        HasChildrenProperty = ExpressionProperty | LengthProperty | AttributeCountProperty | SlotCountProperty | NameCountProperty | FlagsProperty,
    }

    internal static class REvaluationResultPropertiesExtensions {
        private static readonly Dictionary<REvaluationResultProperties, string> _mapping = new Dictionary<REvaluationResultProperties, string> {
            [ExpressionProperty] = FieldNames.Expression,
            [AccessorKindProperty] = FieldNames.AccessorKind,
            [TypeNameProperty] = FieldNames.Type,
            [ClassesProperty] = FieldNames.Classes,
            [LengthProperty] = FieldNames.Length,
            [SlotCountProperty] = FieldNames.SlotCount,
            [AttributeCountProperty] = FieldNames.AttributeCount,
            [NameCountProperty] = FieldNames.NameCount,
            [DimProperty] = FieldNames.Dim,
            [FlagsProperty] = FieldNames.Flags,
            [ComputedValueProperty] = FieldNames.ComputedValue,
            [CanCoerceToDataFrameProperty] = FieldNames.CanCoerceToDataFrame,
        };

        public static string ToRVector(this REvaluationResultProperties properties) {
            var fieldNames = _mapping.Where(kv => properties.HasFlag(kv.Key)).Select(kv => "'" + kv.Value + "'");
            return Invariant($"base::c({string.Join(", ", fieldNames)})");
        }
    }
}
