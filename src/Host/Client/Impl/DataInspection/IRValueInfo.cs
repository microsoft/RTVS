// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.DataInspection {
    /// <summary>
    /// Describes the result of evaluating an expression that produced a value that is not a promise or an active binding. 
    /// </summary>
    /// <remarks>
    /// Note that most properties of the object will only have a meaningful value if the corresponding <see cref="REvaluationResultProperties"/>
    /// flag was specified when producing the result. All properties which were not so requested will be <see langword="null"/>.
    /// </remarks>
    public interface IRValueInfo : IREvaluationResultInfo {
        /// <summary>
        /// String representation of the value.
        /// </summary>
        string Representation { get; }

        /// <summary>
        /// The kind of accessor that was used to obtain this <see cref="IRValueInfo"/> from its parent.
        /// </summary>
        RChildAccessorKind AccessorKind { get; }

        /// <summary>
        /// Type of the value, as computed by <c>typeof(...)</c>.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// List of classes of the value, as computed by <c>classes(...)</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="REvaluationResultProperties.ClassesProperty"/> was not specified, this property will be
        /// <see langword="null"/>, rather than an empty collection.
        /// </remarks>
        IReadOnlyList<string> Classes { get; }

        /// <summary>
        /// Length of the value, as computed by <c>length(...)</c>.
        /// </summary>
        long? Length { get; }

        /// <summary>
        /// Number of attributes that this value has, as computed by <c>length(attributes(...))</c>.
        /// </summary>
        long? AttributeCount { get; }

        /// <summary>
        /// Number of slots that this value has, as computed by <c>length(slotNames(class(...)))</c>.
        /// </summary>
        long? SlotCount { get; }

        /// <summary>
        /// Number of names that the children of value have, as computed by <c>length(names(...))</c>.
        /// </summary>
        long? NameCount { get; }

        /// <summary>
        /// Dimensions that this value has, as computed by <c>dim(...)</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="REvaluationResultProperties.DimProperty"/> was not specified, this property will be
        /// <see langword="null"/>, rather than an empty collection.
        /// </remarks>
        IReadOnlyList<long> Dim { get; }

        /// <summary>
        /// Various miscellaneous flags describing this value.
        /// </summary>
        RValueFlags Flags { get; }

        /// <summary>
        /// <see langword="true"/> if <see cref="REvaluationResultInfoExtensions.DescribeChildrenAsync"/> will return any items,
        /// otherwise <see langword="false"/>.
        /// </summary>
        bool HasChildren { get; }

        /// <summary>
        /// <see langword="true"/> if <see cref="REvaluationResultProperties.ExpressionProperty"/> can be coerced to 
        /// a data frame, <see langword="false"/> otherwise.
        /// </summary>
        bool CanCoerceToDataFrame { get; }
    }

    public static class RValueInfoExtensions {
        /// <seealso cref="RValueFlags.Atomic"/>
        public static bool IsAtomic(this IRValueInfo info) =>
            info.Flags.HasFlag(RValueFlags.Atomic);

        /// <seealso cref="RValueFlags.Recursive"/>
        public static bool IsRecursive(this IRValueInfo info) =>
            info.Flags.HasFlag(RValueFlags.Recursive);

        /// <summary>
        /// Whether this value has any attributes.
        /// </summary>
        public static bool HasAttributes(this IRValueInfo info) =>
            info.AttributeCount != null && info.AttributeCount != 0;

        /// <summary>
        /// Whether this value has any slots.
        /// </summary>
        public static bool HasSlots(this IRValueInfo info) =>
            info.SlotCount != null && info.SlotCount != 0;
    }
}
