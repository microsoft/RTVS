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
    internal class ViewProjectionManager {
        private readonly ITextBuffer _diskBuffer;
        private List<LanguageProjectionManager> _langProjectionManagers;

        public ViewProjectionManager(ITextBuffer diskBuffer, IProjectionBufferFactoryService factory) {
            _diskBuffer = diskBuffer;
            ITextSnapshot diskSnap = diskBuffer.CurrentSnapshot;
            SnapshotSpan everything = new SnapshotSpan(diskSnap, 0, diskSnap.Length);
            ITrackingSpan trackingSpan = diskSnap.CreateTrackingSpan(everything, SpanTrackingMode.EdgeInclusive);
            IContentType projectionContentType = ContentTypeManager.GetContentType(HTMLProjectionContentTypeDefinition.HtmlProjectionContentType);

            ViewBuffer = factory.CreateProjectionBuffer(this,
                    new List<object>(1) { trackingSpan },
                    ProjectionBufferOptions.None,
                    projectionContentType);

            Tracker = new GrowingSpanTracker(_diskBuffer);
            _langProjectionManagers = new List<LanguageProjectionManager>();
        }

        public void Close() {
            Tracker.Close();
        }

        public IProjectionBuffer ViewBuffer { get; }

        /// <summary>
        /// Hook up an embedded language buffer. The mappings specify a span of text in the language buffer and where
        /// it is to be placed into the view buffer. 
        /// </summary>
        public void AddLanguageBuffer(LanguageProjectionManager langManager) {
            if (!_langProjectionManagers.Contains(langManager))
                _langProjectionManagers.Add(langManager);

            if (langManager.MappingCount > 0) {
                UpdateViewBuffer(langManager);
            }
        }

        /// <summary>
        /// Unhook a language buffer from the view buffer.
        /// </summary>
        public void RemoveLanguageBuffer(LanguageProjectionManager langManager) {
            _langProjectionManagers.Remove(langManager);

            Tracker.RemoveBuffer(langManager.LangBuffer);
            if (ViewBuffer.SourceBuffers.Contains(langManager.LangBuffer)) {
                UpdateViewBuffer(langManager);
            }
        }

        bool _inUpdateViewBuffer = false;
        private void UpdateViewBuffer(LanguageProjectionManager langManager) {
            if (_inUpdateViewBuffer) {
                if (ViewBuffer.SourceBuffers.Contains(langManager.LangBuffer)) {
                    // someone is trying to update the view buffer while a call is already in progress. Clear out the view's projections completely,
                    //   forcing the outer update of the view buffer to ensure no embedded languages are mapped and will update the view during 
                    //   their ReplaceSpans call.
                    ITextSnapshot diskSnap = _diskBuffer.CurrentSnapshot;
                    SnapshotSpan everything = new SnapshotSpan(diskSnap, 0, diskSnap.Length);
                    ITrackingSpan trackingSpan = diskSnap.CreateTrackingSpan(everything, SpanTrackingMode.EdgeInclusive);
                    ViewBuffer.ReplaceSpans(0, ViewBuffer.CurrentSnapshot.SpanCount, new List<object>(1) { trackingSpan }, ProjectionBuffer.GetMinimalChangeEditOptions(), this);
                }
            } else {
                _inUpdateViewBuffer = true;

                try {
                    List<object> newSourceSpans = new List<object>();
                    List<LanguageProjectionManager> langProjectionManagersCopy = new List<LanguageProjectionManager>(_langProjectionManagers);

                    // Ensure the tracker is up to date (as the code below depends on
                    //   any disconnected tracking points having been discovered)
                    Tracker.EnsureTrackingPoints();

                    // Notify each of the language managers that we are about to update the view buffer
                    //   and that they should remove any disconnected span datas. Use a copy of _langProjectionManagers
                    //   as the call into RemoveDisconnectedSpanDatas can do an (Add/Remove)LanguageBuffer pair of calls
                    foreach (LanguageProjectionManager curLangManager in langProjectionManagersCopy) {
                        curLangManager.RemoveDisconnectedMappings();
                    }

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
                        ViewBuffer.ReplaceSpans(0, ViewBuffer.CurrentSnapshot.SpanCount, newSourceSpans, ProjectionBuffer.GetMinimalChangeEditOptions(), this);
                    } catch (Exception) { }
                } finally {
                    _inUpdateViewBuffer = false;
                }
            }
        }
    }
}
