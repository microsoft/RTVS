// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Editor.Tree {
    public sealed class TextChange {
        /// <summary>
        /// Text snapshot version
        /// </summary>
        public int Version;

        /// <summary>
        /// Type of pending changes.
        /// </summary>
        public TextChangeType TextChangeType;

        /// <summary>
        /// Changed range in the old snapshot.
        /// </summary>
        public ITextRange OldRange;

        /// <summary>
        /// Changed range in the current snapshot.
        /// </summary>
        public ITextRange NewRange;

        /// <summary>
        /// True if full parse required.
        /// </summary>
        public bool FullParseRequired;

        /// <summary>
        /// Previuos text snapshot
        /// </summary>
        public ITextProvider OldTextProvider;

        /// <summary>
        /// Current text snapshot
        /// </summary>
        public ITextProvider NewTextProvider;

        public TextChange() => Clear();
        public TextChange(TextChange change, ITextProvider newTextProvider) : this() => Combine(change);

        public void Clear() {
            TextChangeType = TextChangeType.Trivial;
            OldRange = TextRange.EmptyRange;
            NewRange = TextRange.EmptyRange;
            FullParseRequired = false;
            OldTextProvider = null;
            NewTextProvider = null;
        }

        /// <summary>
        /// True if no changes are pending.
        /// </summary>
        public bool IsEmpty => !FullParseRequired && OldRange.Length == 0 && NewRange.Length == 0;

        /// <summary>
        /// Combines one text change with another
        /// </summary>
        public void Combine(TextChange other) {
            FullParseRequired |= other.FullParseRequired;
            TextChangeType |= other.TextChangeType;

            if (!other.IsEmpty) {
                OldRange = TextRange.Union(OldRange, other.OldRange);
                NewRange = TextRange.Union(NewRange, other.NewRange);
                OldTextProvider = other.OldTextProvider.Version < this.OldTextProvider.Version ? other.OldTextProvider : this.OldTextProvider;
                NewTextProvider = other.NewTextProvider.Version > this.NewTextProvider.Version ? other.NewTextProvider : this.NewTextProvider;
                Version = Math.Max(this.Version, other.Version);
            }
        }

        /// <summary>
        /// True if pending change does not require background parsing
        /// </summary>
        public bool IsSimpleChange => !FullParseRequired && TextChangeType != TextChangeType.Structure;

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture,
                "Version:{0}, TextChangeType:{1}, OldRange:{2}, NewRange:{3}, FullParseRequired:{4}",
                Version, TextChangeType, OldRange, NewRange, FullParseRequired);
    }
}
