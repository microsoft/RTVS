// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Tree {
    internal class TextChange : ICloneable
    {
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
        public TextRange OldRange;

        /// <summary>
        /// Changed range in the current snapshot.
        /// </summary>
        public TextRange NewRange;

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

        public TextChange()
        {
            Clear();
        }

        public TextChange(TextChange change, ITextProvider newTextProvider)
            : this()
        {
            this.Combine(change);

            ITextSnapshotProvider newSnapshotProvider = newTextProvider as ITextSnapshotProvider;
            ITextSnapshotProvider changeNewSnapshotProvider = change.NewTextProvider as ITextSnapshotProvider;

            if ((newSnapshotProvider != null) && (changeNewSnapshotProvider != null))
            {
                ITextSnapshot changeNewSnapshot = changeNewSnapshotProvider.Snapshot;
                ITextSnapshot newSnapshot = newSnapshotProvider.Snapshot;

                if (changeNewSnapshot.Version.ReiteratedVersionNumber != newSnapshot.Version.ReiteratedVersionNumber)
                {
                    SnapshotSpan changeNewSpan = change.NewRange.ToSnapshotSpan(changeNewSnapshot);
                    Span? oldChangedSpan;
                    Span? newChangedSpan;

                    if (changeNewSnapshot.Version.GetChangedExtent(newSnapshot.Version, out oldChangedSpan, out newChangedSpan))
                    {
                        int start = Math.Min(oldChangedSpan.Value.Start, change.NewRange.Start);
                        int end = Math.Max(oldChangedSpan.Value.End, change.NewRange.End);

                        changeNewSpan = new SnapshotSpan(changeNewSnapshot, Span.FromBounds(start, end));
                    }

                    SnapshotSpan newSpan = changeNewSpan.TranslateTo(newSnapshot, SpanTrackingMode.EdgeInclusive);

                    NewRange = new TextRange(newSpan.Start.Position, newSpan.Length);
                }
            }

            NewTextProvider = newTextProvider;
            Version = NewTextProvider.Version;
        }

        public void Clear()
        {
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
        public bool IsEmpty
        {
            get { return !FullParseRequired && (OldRange.Length == 0 && NewRange.Length == 0); }
        }

        /// <summary>
        /// Combines one text change with another
        /// </summary>
        public void Combine(TextChange other)
        {
            if (other.IsEmpty)
                return;

            FullParseRequired |= other.FullParseRequired;
            TextChangeType |= other.TextChangeType;

            if (OldRange == TextRange.EmptyRange || NewRange == TextRange.EmptyRange)
            {
                OldRange = other.OldRange;
                NewRange = other.NewRange;
                OldTextProvider = other.OldTextProvider;
                NewTextProvider = other.NewTextProvider;
            }
            else
            {
                ITextSnapshotProvider oldSnapshotProvider = OldTextProvider as ITextSnapshotProvider;
                ITextSnapshotProvider newSnapshotProvider = NewTextProvider as ITextSnapshotProvider;
                ITextSnapshotProvider otherOldSnapshotProvider = other.OldTextProvider as ITextSnapshotProvider;
                ITextSnapshotProvider otherNewSnapshotProvider = other.NewTextProvider as ITextSnapshotProvider;
                bool changesAreFromNextSnapshot = false;

                if ((oldSnapshotProvider != null) && (newSnapshotProvider != null) && 
                    (otherOldSnapshotProvider != null) && (otherNewSnapshotProvider != null))
                {
                    ITextSnapshot newSnapshot = newSnapshotProvider.Snapshot;
                    ITextSnapshot otherOldSnapshot = otherOldSnapshotProvider.Snapshot;
                    if (newSnapshot.Version.ReiteratedVersionNumber == otherOldSnapshot.Version.ReiteratedVersionNumber)
                    {
                        changesAreFromNextSnapshot = true;
                    }
                }

                if (!changesAreFromNextSnapshot)
                {
                    // Assume these changes are from the same snapshot
                    int oldStart = Math.Min(other.OldRange.Start, this.OldRange.Start);
                    int oldEnd = Math.Max(other.OldRange.End, this.OldRange.End);

                    int newStart = Math.Min(other.NewRange.Start, this.NewRange.Start);
                    int newEnd = Math.Max(other.NewRange.End, this.NewRange.End);

                    OldRange = TextRange.FromBounds(oldStart, oldEnd);
                    NewRange = TextRange.FromBounds(newStart, newEnd);
                }
                else
                {
                    // the "other" change is from the subsequent snapshot. Merge the changes.
                    ITextSnapshot oldSnapshot = oldSnapshotProvider.Snapshot;
                    ITextSnapshot newSnapshot = newSnapshotProvider.Snapshot;
                    ITextSnapshot otherOldSnapshot = otherOldSnapshotProvider.Snapshot;
                    ITextSnapshot otherNewSnapshot = otherNewSnapshotProvider.Snapshot;

                    SnapshotSpan otherOldSpan = other.OldRange.ToSnapshotSpan(otherOldSnapshot);
                    SnapshotSpan oldSpan = otherOldSpan.TranslateTo(oldSnapshot, SpanTrackingMode.EdgeInclusive);

                    SnapshotSpan newSpan = NewRange.ToSnapshotSpan(newSnapshot);
                    SnapshotSpan otherNewSpan = newSpan.TranslateTo(otherNewSnapshot, SpanTrackingMode.EdgeInclusive);

                    OldRange = new TextRange(TextRange.Union(OldRange, oldSpan.ToTextRange()));
                    NewRange = new TextRange(TextRange.Union(other.NewRange, otherNewSpan.ToTextRange()));
                    NewTextProvider = other.NewTextProvider;
                }
            }

            Version = Math.Max(this.Version, other.Version);
        }

        /// <summary>
        /// True if pending change does not require background parsing
        /// </summary>
        public bool IsSimpleChange
        {
            get
            {
                return !FullParseRequired && TextChangeType != TextChangeType.Structure;
            }
        }

        #region ICloneable Members
        public object Clone()
        {
            TextChange clone = this.MemberwiseClone() as TextChange;

            clone.OldRange = this.OldRange.Clone() as TextRange;
            clone.NewRange = this.NewRange.Clone() as TextRange;

            return clone;
        }
        #endregion

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, 
                "Version:{0}, TextChangeType:{1}, OldRange:{2}, NewRange:{3}, FullParseRequired:{4}", 
                Version, TextChangeType, OldRange, NewRange, FullParseRequired);
        }
    }
}
