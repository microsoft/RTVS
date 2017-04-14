// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;
using static Microsoft.R.Host.Client.REvaluationResult;

namespace Microsoft.R.DataInspection {
    internal sealed class RValueInfo : REvaluationResultInfo, IRValueInfo {
        public string Representation { get; }

        public RChildAccessorKind AccessorKind { get; }

        public string TypeName { get; }

        public IReadOnlyList<string> Classes { get; }

        public long? Length { get; }

        public long? AttributeCount { get; }

        public long? SlotCount { get; }

        public long? NameCount { get; }

        public IReadOnlyList<long> Dim { get; }

        public RValueFlags Flags { get; }

        public bool CanCoerceToDataFrame { get; }

        public bool HasChildren {
            get {
                if (this.HasSlots()) {
                    return true;
                }

                // These have length 1, but are not subsettable, so report no children.
                if (TypeName == "closure" || TypeName == "symbol") {
                    return false;
                }

                if (Length != null) {
                    if (this.IsAtomic()) {
                        // If it is a single-element vector, do not list the element as a child, because it is identical
                        // to the vector itself. However, if the element is named, list it to provide access to the name.
                        if (Length > 1 || (NameCount != null && NameCount != 0)) {
                            return true;
                        }
                    } else {
                        if (Length != 0) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        internal RValueInfo(
            IRExpressionEvaluator evaluator,
            string environmentExpression,
            string expression,
            string name,
            string representation,
            RChildAccessorKind accessorKind,
            string typeName,
            IReadOnlyList<string> classes,
            long? length,
            long? attributeCount,
            long? slotCount,
            long? nameCount,
            IReadOnlyList<long> dim,
            RValueFlags flags,
            bool canCoerceToDataFrame
        ) : base(evaluator, environmentExpression, expression, name) {

            Representation = representation;
            AccessorKind = accessorKind;
            TypeName = typeName;
            Classes = classes;
            Length = length;
            AttributeCount = attributeCount;
            SlotCount = slotCount;
            NameCount = nameCount;
            Dim = dim;
            Flags = flags;
            CanCoerceToDataFrame = canCoerceToDataFrame;
        }

        internal RValueInfo(IRExpressionEvaluator evaluator, string environmentExpression, string expression, string name, JObject json)
            : base(evaluator, environmentExpression, expression, name) {

            Representation = json.Value<string>(FieldNames.Repr);
            TypeName = json.Value<string>(FieldNames.Type);
            Length = json.Value<long?>(FieldNames.Length);
            AttributeCount = json.Value<long?>(FieldNames.AttributeCount);
            SlotCount = json.Value<long?>(FieldNames.SlotCount);
            NameCount = json.Value<long?>(FieldNames.NameCount);
            CanCoerceToDataFrame = json.Value<bool?>(FieldNames.CanCoerceToDataFrame) ?? false;

            var classes = json.Value<JArray>(FieldNames.Classes);
            if (classes != null) {
                Classes = classes.Select(t => t.Value<string>()).ToArray();
            }

            var dim = json.Value<JArray>(FieldNames.Dim);
            if (dim != null) {
                Dim = dim.Select(t => t.Value<long>()).ToArray();
            }

            var kind = json.Value<string>(FieldNames.AccessorKind);
            switch (kind) {
                case null:
                    AccessorKind = RChildAccessorKind.None;
                    break;
                case "[[":
                    AccessorKind = RChildAccessorKind.Brackets;
                    break;
                case "$":
                    AccessorKind = RChildAccessorKind.Dollar;
                    break;
                case "@":
                    AccessorKind = RChildAccessorKind.At;
                    break;
                default:
                    throw new InvalidDataException(Invariant($"Invalid kind '{kind}' in:\n\n{json}"));
            }

            var flags = json.Value<JArray>(FieldNames.Flags)?.Select(v => v.Value<string>());
            if (flags != null) {
                foreach (var flag in flags) {
                    switch (flag) {
                        case "atomic":
                            Flags |= RValueFlags.Atomic;
                            break;
                        case "recursive":
                            Flags |= RValueFlags.Recursive;
                            break;
                        case "has_parent_env":
                            Flags |= RValueFlags.HasParentEnvironment;
                            break;
                        default:
                            throw new InvalidDataException(Invariant($"Unrecognized flag '{flag}' in:\n\n{json}"));
                    }
                }
            }
        }

        public override IREvaluationResultInfo ToEnvironmentIndependentResult() =>
            new RValueInfo(Evaluator, EnvironmentExpression, this.GetEnvironmentIndependentExpression(), Name, Representation,
                AccessorKind, TypeName, Classes, Length, AttributeCount, SlotCount, NameCount, Dim, Flags, CanCoerceToDataFrame);
    }
}
