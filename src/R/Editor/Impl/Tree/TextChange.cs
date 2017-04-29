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
        public int Version { get; set; }

        /// <summary>
        /// Type of pending changes.
        /// </summary>
        public TextChangeType TextChangeType { get; set; }

        /// <summary>
        /// Changed range in the old snapshot.
        /// </summary>
        public ITextRange OldRange { get; set; }

        /// <summary>
        /// Changed range in the current snapshot.
        /// </summary>
        public ITextRange NewRange { get; set; }

        /// <summary>
        /// True if full parse required.
        /// </summary>
        public bool FullParseRequired { get; set; }

        /// <summary>
        /// Previuos text snapshot
        /// </summary>
        public ITextProvider OldTextProvider { get; set; }

        /// <summary>
        /// Current text snapshot
        /// </summary>
        public ITextProvider NewTextProvider { get; set; }

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

                if (OldTextProvider == null) {
                    OldTextProvider = other.OldTextProvider;
                } else {
                    OldTextProvider = other.OldTextProvider.Version < OldTextProvider.Version ? other.OldTextProvider : OldTextProvider;
                }

                if (NewTextProvider == null) {
                    NewTextProvider = other.NewTextProvider;
                } else {
                    NewTextProvider = other.NewTextProvider.Version > NewTextProvider.Version ? other.NewTextProvider : NewTextProvider;
                }
                Version = NewTextProvider.Version;
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

        public TextChange Clone() => new TextChange() {
            TextChangeType = this.TextChangeType,
            OldRange = this.OldRange,
            NewRange = this.NewRange,
            FullParseRequired = this.FullParseRequired,
            OldTextProvider = this.OldTextProvider.Clone(),
            NewTextProvider = this.NewTextProvider.Clone()
        };
    }
}
