// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// Holds various representations of an R value.
    /// </summary>
    /// <remarks>
    /// For the fields of this structure to be filled, the corresponding <see cref="DebugEvaluationResultFields"/>
    /// flags need to be specified when producing the <see cref="DebugValueEvaluationResult"/>.
    /// </remarks>
    /// <seealso cref="DebugValueEvaluationResult.GetRepresentation"/>
    public struct DebugValueRepresentation {
        /// <summary>
        /// Representation of the value obtained by calling <c>deparse()</c>.
        /// </summary>
        /// <seealso cref="DebugEvaluationResultFields.ReprDeparse"/>
        public readonly string Deparse;

        /// <summary>
        /// Representation of the value obtained by calling <c>toString()</c>.
        /// </summary>
        /// <seealso cref="DebugEvaluationResultFields.ReprToString"/>
        public readonly new string ToString;

        /// <summary>
        /// Representation of the value obtained by calling <c>str()</c>.
        /// </summary>
        /// <seealso cref="DebugEvaluationResultFields.ReprStr"/>
        public readonly string Str;

        internal DebugValueRepresentation(JObject repr) {
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
