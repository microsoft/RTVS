// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.DataInspection {
    internal sealed class RValueInfo : REvaluationInfo, IRValueInfo {
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

            Representation = json.Value<string>("repr");
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

            var flags = json.Value<JArray>("flags")?.Select(v => v.Value<string>());
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
