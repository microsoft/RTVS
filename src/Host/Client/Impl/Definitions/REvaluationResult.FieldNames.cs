// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    partial struct REvaluationResult {
        public class FieldNames {
            public const string Repr = "repr";
            public const string Expression = "expression";
            public const string AccessorKind = "kind";
            public const string Type = "type";
            public const string Classes = "classes";
            public const string Length = "length";
            public const string SlotCount = "slot_count";
            public const string AttributeCount = "attr_count";
            public const string NameCount = "name_count";
            public const string Dim = "dim";
            public const string Flags = "flags";
            public const string ComputedValue = "computed_value";
            public const string CanCoerceToDataFrame = "to_df";
            public const string Error = "error";
            public const string Promise = "promise";
            public const string ActiveBinding = "active_binding";
        }
    }
}
