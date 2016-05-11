// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Nodes {
    public abstract class TreeNode : ITextRange {
        /// <summary>
        /// Node name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Node prefix
        /// </summary>
        public abstract string Prefix { get; }

        /// <summary>
        /// Node fully qialified name (prefix:name)
        /// </summary>
        public abstract string QualifiedName { get; }

        /// <summary>
        /// Node name text range
        /// </summary>
        public abstract ITextRange NameRange { get; }

        /// <summary>
        /// Node prefix text range
        /// </summary>
        public abstract ITextRange PrefixRange { get; }

        /// <summary>
        /// Range of the qualified name (prefix:name)
        /// </summary>
        public abstract ITextRange QualifiedNameRange { get; }

        /// <summary>
        /// Determines if node name has namespace prefix
        /// </summary>
        public abstract bool HasPrefix();

        /// <summary>
        /// Start of the node text range
        /// </summary>
        public abstract int Start { get; }
        /// <summary>
        /// End of the node text range (exclusive)
        /// </summary>
        public abstract int End { get; }

        /// <summary>
        /// Length of the node text range
        /// </summary>
        public int Length { get { return End - Start; } }

        /// <summary>
        /// Determines if node contains given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        public virtual bool Contains(int position) { return position >= Start && position < End; }

        /// <summary>
        /// Node outer range
        /// </summary>
        public virtual ITextRange OuterRange { get { return TextRange.FromBounds(Start, End); } }

        /// <summary>
        /// Node inner range (between start and end tags). Can be empty for self-closed elements.
        /// </summary>
        public abstract ITextRange InnerRange { get; }

        #region Positions

        /// <summary>Shifts node start, end and all child elements by the specified offset.</summary>
        public abstract void Shift(int offset);

        /// <summary>
        /// Shifts node components that are located at or beyond given start point by the specified range
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="offset">Offset to shift by</param>
        public abstract void ShiftStartingFrom(int position, int offset);

        #endregion

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            return String.Format(CultureInfo.CurrentCulture, "<{0}  [{1}...{2}", QualifiedName, Start, End);
        }
    }
}
