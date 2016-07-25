// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    internal sealed class RValueInfo : REvaluationResultInfo, IRValueInfo {
        public string Representation { get; }

        public RChildAccessorKind AccessorKind { get; }

        public string TypeName { get; }

        public IReadOnlyList<string> Classes { get; }

        public int? Length { get; }

        public int? AttributeCount { get; }

        public int? SlotCount { get; }

        public int? NameCount { get; }

        public IReadOnlyList<int> Dim { get; }

        public RValueFlags Flags { get; }

        public bool CanExportToCsv { get; }

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

        internal RValueInfo(IRSession session, string environmentExpression, string expression, string name, JObject json)
            : base(session, environmentExpression, expression, name) {

            Representation = json.Value<string>(REvaluationResultFieldNames.ReprFieldName);
            TypeName = json.Value<string>(REvaluationResultFieldNames.TypeFieldName);
            Length = json.Value<int?>(REvaluationResultFieldNames.LengthFieldName);
            AttributeCount = json.Value<int?>(REvaluationResultFieldNames.AttributeCountFieldName);
            SlotCount = json.Value<int?>(REvaluationResultFieldNames.SlotCountFieldName);
            NameCount = json.Value<int?>(REvaluationResultFieldNames.NameCountFieldName);
            CanExportToCsv = json.Value<bool>(REvaluationResultFieldNames.CanExportToCsvFieldName);

            var classes = json.Value<JArray>(REvaluationResultFieldNames.ClassesFieldName);
            if (classes != null) {
                Classes = classes.Select(t => t.Value<string>()).ToArray();
            }

            var dim = json.Value<JArray>(REvaluationResultFieldNames.DimFieldName);
            if (dim != null) {
                Dim = dim.Select(t => t.Value<int>()).ToArray();
            }

            var kind = json.Value<string>(REvaluationResultFieldNames.AccessorKindFieldName);
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

            var flags = json.Value<JArray>(REvaluationResultFieldNames.FlagsFieldName)?.Select(v => v.Value<string>());
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
    }
}
