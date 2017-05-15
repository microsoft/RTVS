// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using static System.FormattableString;

namespace Microsoft.R.Editor.Tree {
    public sealed class TreeTextChange: TextChange {
        /// <summary>
        /// Text snapshot version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Type of pending changes.
        /// </summary>
        public TextChangeType TextChangeType { get; set; }

        /// <summary>
        /// True if full parse required.
        /// </summary>
        public bool FullParseRequired { get; set; }

        public TreeTextChange(TextChange tc) :
            base(tc.Start, tc.OldLength, tc.NewLength, tc.OldTextProvider, tc.NewTextProvider) { }

        public TreeTextChange(int start, int oldLength, int newLength, ITextProvider oldTextProvider, ITextProvider newTextProvider):
            base(start, oldLength, newLength, oldTextProvider, newTextProvider) { }

        public override void Clear() {
            base.Clear();
            TextChangeType = TextChangeType.Trivial;
            FullParseRequired = false;
        }

        /// <summary>
        /// True if no changes are pending.
        /// </summary>
        public bool IsEmpty => !FullParseRequired && OldRange.Length == 0 && NewRange.Length == 0;

        /// <summary>
        /// Combines one text change with another
        /// </summary>
        public void Combine(TreeTextChange other) {
            TextChangeType = MathExtensions.Max(TextChangeType, other.TextChangeType);
            FullParseRequired |= other.FullParseRequired || TextChangeType == TextChangeType.Structure;

            // Combine two sequential changes into one. Note that damaged regions
            // typically don't shrink. There are some exceptions such as when
            // text was added and then deleted making the change effectively a no-op,
            // but this is not a typical case. For simplicity and fidelity
            // we'd rather parse more than less.

            if (FullParseRequired) {
                Start = 0;
                OldLength = OldTextProvider?.Length ?? 0;
                NewLength = NewTextProvider?.Length ?? 0;
                return;
            }

            var oldEnd = Math.Max(OldEnd, other.OldEnd);
            var newEnd = Math.Max(NewEnd, other.NewEnd);

            Start = OldTextProvider != null ? Math.Min(this.Start, other.Start) : other.Start;
            NewLength = Math.Max(newEnd, other.NewEnd) - Start;
            OldLength = Math.Max(oldEnd, other.OldEnd) - Start;

            Debug.Assert(OldLength >= 0);
            Debug.Assert(NewLength >= 0);
            Debug.Assert(Start <= OldEnd);
            Debug.Assert(Start <= NewEnd);

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

            NewLength = Math.Min(NewLength, NewTextProvider.Length);
            OldLength = Math.Min(OldLength, OldTextProvider.Length);

            Version = NewTextProvider.Version;
        }

        /// <summary>
        /// True if pending change does not require background parsing
        /// </summary>
        public bool IsSimpleChange => !FullParseRequired && TextChangeType != TextChangeType.Structure;

        public override string ToString()
            => Invariant($"Version:{Version}, TextChangeType:{TextChangeType}, OldRange:{OldRange}, NewRange:{NewRange}, FullParseRequired:{FullParseRequired}");
    }
}
