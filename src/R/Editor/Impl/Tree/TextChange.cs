// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Languages.Core.Text;
using static System.FormattableString;

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
        /// Start position of the change
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        ///  End of the affected segment in the original snapshot.
        /// </summary>
        public int OldEnd { get; set; }

        /// <summary>
        /// End of the replacement segment in the new snapshot.
        /// </summary>
        public int NewEnd { get; set; }

        /// <summary>
        ///  Length of the affected segment in the original snapshot.
        /// </summary>
        public int OldLength => OldEnd - Start;

        /// <summary>
        /// Length of the replacement segment in the new snapshot.
        /// </summary>
        public int NewLength => NewEnd - Start;

        /// <summary>
        /// Changed range in the old snapshot.
        /// </summary>
        public ITextRange OldRange => new TextRange(Start, OldLength);

        /// <summary>
        /// Changed range in the current snapshot.
        /// </summary>
        public ITextRange NewRange => new TextRange(Start, NewLength);

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

        public TextChange() : this(0, 0, 0, null, null) { }

        public TextChange(int start, int oldLength, int newLength, ITextProvider oldTextProvider, ITextProvider newTextProvider) {
            Start = start;
            OldEnd = start + oldLength;
            NewEnd = start + newLength;
            OldTextProvider = oldTextProvider;
            NewTextProvider = newTextProvider;
        }

        public void Clear() {
            Start = OldEnd = NewEnd = 0;
            OldTextProvider = NewTextProvider = null;
        }

        /// <summary>
        /// True if no changes are pending.
        /// </summary>
        public bool IsEmpty => !FullParseRequired && OldRange.Length == 0 && NewRange.Length == 0;

        /// <summary>
        /// Combines one text change with another
        /// </summary>
        public void Combine(TextChange other) {
            TextChangeType = MathExtensions.Max(TextChangeType, other.TextChangeType);
            FullParseRequired |= other.FullParseRequired || TextChangeType == TextChangeType.Structure;

            // Combine two sequential changes into one. Note that damaged regions
            // typically don't shrink. There are some exceptions such as when
            // text was added and then deleted making the change effectively a no-op,
            // but this is not a typical case. For simplicity and fidelity
            // we'd rather parse more than less.

            if (FullParseRequired) {
                Start = 0;
                OldEnd = OldTextProvider?.Length ?? 0;
                NewEnd = NewTextProvider?.Length ?? 0;
                return;
            }

            // Start point is always the lowest of the two. We need to detemine new largest extent of the change.
            // Translate new change's oldEnd and newEnd to the current change coordinates.
            var oldEnd = TranslatePosition(other.OldEnd, Start, OldEnd, NewEnd);
            var newEnd = TranslatePosition(other.NewEnd, Start, OldEnd, NewEnd);

            // Damaged region never shrinks
            oldEnd = Math.Max(this.OldEnd, oldEnd);
            newEnd = Math.Max(this.NewEnd, newEnd);

            Start = Math.Min(this.Start, other.Start);
            NewEnd = Math.Min(newEnd, NewTextProvider?.Length ?? newEnd);
            OldEnd = Math.Min(oldEnd, OldTextProvider?.Length ?? oldEnd);

            Debug.Assert(Start <= OldEnd);
            Debug.Assert(OldEnd <= NewEnd);

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

            Version = NewTextProvider?.Version ?? Version;
        }

        private static int TranslatePosition(int point, int start, int oldLength, int newLength)
            // Note that length is reversed: if current change DELETED character, translation
            // must compensate and INCREASE size of the region.
            => point < start + oldLength ? point : point + (oldLength - newLength);

        /// <summary>
        /// True if pending change does not require background parsing
        /// </summary>
        public bool IsSimpleChange => !FullParseRequired && TextChangeType != TextChangeType.Structure;

        public override string ToString()
            => Invariant($"Version:{Version}, TextChangeType:{TextChangeType}, OldRange:{OldRange}, NewRange:{NewRange}, FullParseRequired:{FullParseRequired}");
    }
}
