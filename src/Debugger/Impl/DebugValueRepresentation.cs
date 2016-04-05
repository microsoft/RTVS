// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    public struct DebugValueRepresentation {
        public readonly string Deparse;
        public readonly new string ToString;
        public readonly string Str;

        public DebugValueRepresentation(JObject repr, DebugValueRepresentationKind kind) {
            Deparse = repr.Value<string>("deparse");
            ToString = repr.Value<string>("toString");
            Str = repr.Value<string>("str");
        }

        public override bool Equals(object obj) =>
            base.Equals(obj) || (obj as IEquatable<DebugValueRepresentation>)?.Equals(this) == true;

        public override int GetHashCode() =>
            base.GetHashCode();
    }
}
