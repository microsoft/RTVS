// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Represents top level buffer in the buffer graph. Projection buffer is a stitched buffer 
    /// that maps to contained language(s) as well as into the disk buffer. Example: HTML file
    /// comes as a disk buffer and gets split into HTML, CSS, JavaScript. The respective contained
    /// language buffers are generated (CSS and JavaScript) Then the projection buffer is created 
    /// that contains stitched regions: some from HTML, some from CSS and some from JavaScript buffers.
    /// Similarly, R inside R Markdown is a contained language. Original RMD is in the disk buffer
    /// but actual buffer we see in the view is the projection buffer.
    /// </summary>
    public abstract class ProjectionBuffer {
        public IProjectionBuffer IProjectionBuffer { get; protected set; }

        /// <summary>
        /// Original top-level file content
        /// </summary>
        protected ITextBuffer DiskBuffer { get; set; }

        internal ProjectionBuffer(ITextBuffer diskBuffer) {
            DiskBuffer = diskBuffer;
        }

        internal static EditOptions GetAppropriateChangeEditOptions(ReadOnlyCollection<SnapshotSpan> oldSourceSpans, List<object> newSourceSpans) {
            // A bunch of mappings are going to be either added or removed. This can be a real perf killer 
            // in the subsequent call to ReplaceSpans due to the diffing of the source text of the old/new spans.
            // We typically send over minimal change notifications for the benefit of the contained language, 
            // as it helps them determine a better context for what has changed. In this case though,
            // we know for certain they are going to be receiving a fair number of disparate changes, 
            // so it's not worth the effort (and performance impact) of sending over minimal changes.
            if (Math.Abs(oldSourceSpans.Count - newSourceSpans.Count) > 10) {
                return GetNonMinimalChangeEditOptions();
            }

            int oldLen = 0;
            foreach (SnapshotSpan curSpan in oldSourceSpans) {
                oldLen += curSpan.Length;
            }

            int newLen = 0;
            foreach (object curSourceSpan in newSourceSpans) {
                if (curSourceSpan is string) {
                    newLen += (curSourceSpan as string).Length;
                } else if (curSourceSpan is ITrackingSpan) {
                    ITrackingSpan curTrackingSpan = curSourceSpan as ITrackingSpan;
                    newLen += curTrackingSpan.GetSpan(curTrackingSpan.TextBuffer.CurrentSnapshot).Length;
                }
            }

            // There was a bunch of coded either added or removed from the projection. This also will 
            //   kill the diffing code's performance, so again, we'll just tell them not to do a minimal diff.
            if (Math.Abs(oldLen - newLen) > 10000) {
                return GetNonMinimalChangeEditOptions();
            }

            return GetMinimalChangeEditOptions();
        }

        internal static EditOptions GetNonMinimalChangeEditOptions() {
            StringDifferenceOptions differenceOptions = new StringDifferenceOptions(EditOptions.DefaultMinimalChange.DifferenceOptions);
            return new EditOptions(false, differenceOptions);
        }

        internal static EditOptions GetMinimalChangeEditOptions() {
            StringDifferenceOptions differenceOptions = new StringDifferenceOptions(EditOptions.DefaultMinimalChange.DifferenceOptions);
            return new EditOptions(differenceOptions);
        }
    }
}
