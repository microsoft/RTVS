// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Outline {
    /// <summary>
    /// Base outline (collapsible regions) builder. Performs region building
    /// in a background thread that starts on idle so it does not run
    /// continuously as user is typing or otherwise constantly changing 
    /// the underlying text buffer content.
    /// </summary>
    public abstract class OutlineRegionBuilder : IDisposable {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public event EventHandler<OutlineRegionsChangedEventArgs> RegionsChanged;

        protected OutlineRegionCollection CurrentRegions { get; set; }
        protected IdleTimeAsyncTask BackgroundTask { get; set; }
        protected ITextBuffer TextBuffer { get; set; }
        protected virtual bool IsEnabled { get; } = true;
        protected ICoreShell Shell { get; }

        private long _disposed = 0;
        private readonly object _regionsLock = new object();

        protected OutlineRegionBuilder(ITextBuffer textBuffer, ICoreShell shell) {
            Shell = shell;
            CurrentRegions = new OutlineRegionCollection(0);

            TextBuffer = textBuffer;
            TextBuffer.Changed += OnTextBufferChanged;

            BackgroundTask = new IdleTimeAsyncTask(TaskAction, MainThreadAction, shell);
            if (IsEnabled) {
                BackgroundTask.DoTaskOnIdle(300);
            }
        }

        public virtual bool IsReady => !BackgroundTask.TaskRunning;

        protected virtual void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            // In order to provide nicer experience when user presser and holds
            // ENTER or DELETE or just types really fast, we are going to track
            // regions optimistically and report changes without going through
            // async or idle processing. Idle/async is still going to hit later.

            if (IsEnabled && e.Changes.Count > 0) {
                int start, oldLength, newLength;
                TextUtility.CombineChanges(e, out start, out oldLength, out newLength);

                int changeStart = Int32.MaxValue;
                int changeEnd = 0;

                lock (_regionsLock) {
                    // Remove affected regions and shift the remaining ones. Outlining 
                    // regions are not sorted and can overlap. Hence linear search.

                    for (int i = 0; i < CurrentRegions.Count; i++) {
                        var region = CurrentRegions[i];

                        if (region.End <= start) {
                            continue;
                        }

                        if (region.Contains(start) && region.Contains(start + oldLength)) {
                            region.Expand(0, newLength - oldLength);
                        } else if (region.Start >= start + oldLength) {
                            region.Shift(newLength - oldLength);
                        } else {
                            changeStart = Math.Min(changeStart, region.Start);
                            changeEnd = Math.Max(changeEnd, region.End);

                            CurrentRegions.RemoveAt(i);
                            i--;

                            if (e.Changes.Count > 0) {
                                // If we merged changes, this might be an overaggressive delete. Ensure
                                //   that we'll do a full recalculation later.
                                BackgroundTask.DoTaskOnIdle();
                            }
                        }
                    }
                }

                // If there were previously any regions, make sure we notify our listeners of the changes
                if ((CurrentRegions.Count > 0) || (changeStart < Int32.MaxValue)) {
                    CurrentRegions.TextBufferVersion = TextBuffer.CurrentSnapshot.Version.VersionNumber;
                    if (RegionsChanged != null) {
                        changeEnd = (changeStart == Int32.MaxValue ? changeStart : changeEnd);
                        RegionsChanged(this, new OutlineRegionsChangedEventArgs(CurrentRegions, TextRange.FromBounds(changeStart, changeEnd)));
                    }
                }
            }
        }

        public abstract bool BuildRegions(OutlineRegionCollection newRegions);

        protected bool IsDisposed {
            get { return Interlocked.Read(ref _disposed) > 0; }
        }

        protected virtual object TaskAction() {
            if (!IsDisposed) {
                var snapshot = TextBuffer.CurrentSnapshot;
                var newRegions = new OutlineRegionCollection(snapshot.Version.VersionNumber);

                bool regionsBuilt = BuildRegions(newRegions);
                if (regionsBuilt) {
                    lock (_regionsLock) {
                        var changedRange = CompareRegions(newRegions, CurrentRegions, snapshot.Length);
                        return new OutlineRegionsChange(changedRange, newRegions);
                    }
                }
            }

            return null;
        }

        protected virtual void MainThreadAction(object backgroundProcessingResult) {
            if (!IsDisposed) {
                var result = backgroundProcessingResult as OutlineRegionsChange;

                if (result != null && TextRange.IsValid(result.ChangedRange)) {
                    lock (_regionsLock) {
                        CurrentRegions = result.NewRegions;
                    }

                    RegionsChanged?.Invoke(this,
                        new OutlineRegionsChangedEventArgs(CurrentRegions.Clone() as OutlineRegionCollection,
                        result.ChangedRange)
                     );
                }
            }
        }

        protected virtual ITextRange CompareRegions(
            OutlineRegionCollection newRegions,
            OutlineRegionCollection oldRegions, int upperBound) {
            TextRangeCollection<OutlineRegion> oldClone = null;
            TextRangeCollection<OutlineRegion> newClone = null;

            if (oldRegions != null) {
                oldClone = oldRegions.Clone() as OutlineRegionCollection;
                oldClone.Sort();
            }

            newClone = newRegions.Clone() as OutlineRegionCollection;
            newClone.Sort();

            return newClone.RangeDifference(oldClone, 0, upperBound);
        }

        #region IDisposable Members
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            Interlocked.Exchange(ref _disposed, 1);

            if (TextBuffer != null) {
                TextBuffer.Changed -= OnTextBufferChanged;
                TextBuffer = null;
            }

            if (BackgroundTask != null) {
                BackgroundTask.Dispose();
                BackgroundTask = null;
            }
        }
        #endregion
    }
}
