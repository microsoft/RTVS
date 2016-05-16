// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// This class manages the projection buffer for a primary
    /// language document as well as buffers of contained languages.
    /// </summary>
    internal class PrimaryLanguageProjectionManager: IProjectionEditResolver {
        private readonly ITextBuffer _diskBuffer;
        private List<ContainedLanguageProjectionManager> _langProjectionManagers;

        public PrimaryLanguageProjectionManager(ITextBuffer diskBuffer, IProjectionBufferFactoryService factory, IContentType projectionContentType) {
            _diskBuffer = diskBuffer;
            ITextSnapshot diskSnap = diskBuffer.CurrentSnapshot;
            SnapshotSpan everything = new SnapshotSpan(diskSnap, 0, diskSnap.Length);
            ITrackingSpan trackingSpan = diskSnap.CreateTrackingSpan(everything, SpanTrackingMode.EdgeInclusive);

            ProjectionBuffer = factory.CreateProjectionBuffer(this, new List<object>(1) { trackingSpan }, 
                                                        ProjectionBufferOptions.None, projectionContentType);
            _langProjectionManagers = new List<ContainedLanguageProjectionManager>();
        }

        public IProjectionBuffer ProjectionBuffer { get; }

        /// <summary>
        /// Hook up an embedded language buffer. The mappings specify a span of text in the language buffer and where
        /// it is to be placed into the view buffer. 
        /// </summary>
        public void AddLanguageBuffer(ContainedLanguageProjectionManager langManager) {
            if (!_langProjectionManagers.Contains(langManager))
                _langProjectionManagers.Add(langManager);

            if (langManager.MappingCount > 0) {
                UpdateViewBuffer(langManager);
            }
        }

        /// <summary>
        /// Unhook a language buffer from the view buffer.
        /// </summary>
        public void RemoveLanguageBuffer(ContainedLanguageProjectionManager langManager) {
            _langProjectionManagers.Remove(langManager);
            if (ProjectionBuffer.SourceBuffers.Contains(langManager.LanguageBuffer)) {
                UpdateViewBuffer(langManager);
            }
        }

        bool _inUpdateViewBuffer = false;
        private void UpdateViewBuffer(ContainedLanguageProjectionManager langManager) {
            if (_inUpdateViewBuffer) {
                if (ProjectionBuffer.SourceBuffers.Contains(langManager.LanguageBuffer)) {
                    // someone is trying to update the view buffer while a call is already in progress. Clear out the view's projections completely,
                    //   forcing the outer update of the view buffer to ensure no embedded languages are mapped and will update the view during 
                    //   their ReplaceSpans call.
                    ITextSnapshot diskSnap = _diskBuffer.CurrentSnapshot;
                    SnapshotSpan everything = new SnapshotSpan(diskSnap, 0, diskSnap.Length);
                    ITrackingSpan trackingSpan = diskSnap.CreateTrackingSpan(everything, SpanTrackingMode.EdgeInclusive);
                    ProjectionBuffer.ReplaceSpans(0, ProjectionBuffer.CurrentSnapshot.SpanCount, new List<object>(1) { trackingSpan },
                                                  ProjectionBufferBase.GetMinimalChangeEditOptions(), this);
                }
            } else {
                _inUpdateViewBuffer = true;

                try {
                    List<object> newSourceSpans = new List<object>();
                    List<ContainedLanguageProjectionManager> langProjectionManagersCopy = new List<ContainedLanguageProjectionManager>(_langProjectionManagers);

                    // Add tracking spans from the Tracker's SpanData
                    foreach (GrowingSpanData curTrackerData in Tracker.SpanData) {
                        if (curTrackerData.ProjectionBuffer == null) {
                            // The GrowingSpanData indicates it originates from the html buffer, use it directly
                            newSourceSpans.Add(curTrackerData.TrackingSpan);
                        } else {
                            // The GrowingSpanData indicates it originates from a projection buffer, use
                            //   the projection tracking span it maintains
                            newSourceSpans.Add(curTrackerData.ProjectionTrackingSpan);
                        }
                    }

                    // Workaround for Razor bug where we can get overlapped spans (IIS OOB 37397).
                    try {
                        ProjectionBuffer.ReplaceSpans(0, ProjectionBuffer.CurrentSnapshot.SpanCount, newSourceSpans, ProjectionBufferBase.GetMinimalChangeEditOptions(), this);
                    } catch (Exception) { }
                } finally {
                    _inUpdateViewBuffer = false;
                }
            }
        }
    }
}
