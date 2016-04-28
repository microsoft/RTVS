// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Debugger {
    public interface IDebugValueEvaluationResult: IDebugEvaluationResult {
        /// <summary>
        /// String representation of the value.
        /// </summary>
        string Representation { get; }

        /// <summary>
        /// The kind of accessor that was used to obtain this <see cref="DebugValueEvaluationResult"/> from its parent.
        /// </summary>
        DebugChildAccessorKind AccessorKind { get; }
        /// <summary>
        /// Type of the value, as computed by <c>typeof(...)</c>.
        /// </summary>
        string TypeName { get; }
        /// <summary>
        /// List of classes of the value, as computed by <c>classes(...)</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="DebugEvaluationResultFields.Classes"/> was not specified, this property will be
        /// <see langword="null"/>, rather than an empty collection.
        /// </remarks>
        IReadOnlyList<string> Classes { get; }
        /// <summary>
        /// Length of the value, as computed by <c>length(...)</c>.
        /// </summary>
        int? Length { get; }
        /// <summary>
        /// Number of attributes that this value has, as computed by <c>length(attributes(...))</c>.
        /// </summary>
        int? AttributeCount { get; }
        /// <summary>
        /// Number of slots that this value has, as computed by <c>length(slotNames(class(...)))</c>.
        /// </summary>
        int? SlotCount { get; }
        /// <summary>
        /// Number of names that the children of value have, as computed by <c>length(names(...))</c>.
        /// </summary>
        int? NameCount { get; }
        /// <summary>
        /// Dimensions that this value has, as computed by <c>dim(...)</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="DebugEvaluationResultFields.Dim"/> was not specified, this property will be
        /// <see langword="null"/>, rather than an empty collection.
        /// </remarks>
        IReadOnlyList<int> Dim { get; }
        /// <summary>
        /// Various miscellaneous flags describing this value.
        /// </summary>
        DebugValueEvaluationResultFlags Flags { get; }
    }
}
