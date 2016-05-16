// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// This class manages the projection buffer for an embedded language.
    /// </summary>
    internal class LanguageProjectionManager {
        private ViewProjectionManager ViewManager { get; set; }
        private ITextBuffer DiskBuffer { get; set; }
        public IProjectionBuffer LangBuffer { get; private set; }
        private IProjectionEditResolver EditResolver { get; set; }
        private List<GrowingSpanData> GrowingSpanDatas { get; set; }

        public LanguageProjectionManager(ITextBuffer diskBuffer, ViewProjectionManager viewManager, IContentType contentType) {
            IProjectionBufferFactoryService projectionBufferFactoryService = EditorShell.Current.ExportProvider.GetExport<IProjectionBufferFactoryService>().Value;

            DiskBuffer = diskBuffer;
            LangBuffer = projectionBufferFactoryService.CreateProjectionBuffer(this, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);
            ViewManager = viewManager;
            EditResolver = new LanguageEditResolver(DiskBuffer);
        }

        public int MappingCount {
            get { return GrowingSpanDatas != null ? GrowingSpanDatas.Count : 0; }
        }

        #region Set(Text/Mappings) methods

        public void SetTextAndMappings(string text, ProjectionMapping[] mappings) {
            SetMappingsHelper(mappings, text);
        }

        public void SetMappings(ProjectionMapping[] mappings) {
            if (LangBuffer != null) {
                ITextSnapshot langSnapshot = LangBuffer.CurrentSnapshot;
                string langText = langSnapshot.GetText();

                SetMappingsHelper(mappings, langText);
            }
        }

        public void ResetMappings() {
            if (LangBuffer != null) {
                // This is a bit lame, as we've already set the content type and the content/mappings
                //   of the projection buffer. However, C# depends on a call to their SetColorizer method
                //   from the env LanguageServiceClassifier to actually do any colorization. 
                //   This only happens when the LanguageServiceClassificationTagger.TryCreateTagger
                //   method succeeds, which can only happen when TextDocData.OnTextBufferInitialized
                //   has been completed. However, this isn't called until ServerContainedLanguageSupport.EnsureBufferCoordinator
                //   at which point we've already set the content/mappings. Fix this by forcing
                //   a call to TryCreateTagger by changing the content type instead of resetting the projections
                //   as we used to do as this is *slow* due to platform's diffing code.
                IContentType inertContentType = ContentTypeManager.GetContentType("inert");
                IContentType oldContentType = LangBuffer.ContentType;

                LangBuffer.ChangeContentType(inertContentType, null);
                LangBuffer.ChangeContentType(oldContentType, null);
            }
        }

        /// <summary>
        /// Uses the current GrowingSpanDatas content to create a list of
        /// source spans and inert text spans. This should really only be called from UpdateTextBuffer.
        /// </summary>
        private List<object> CreateSourceSpans(ProjectionMapping[] mappings, string fullLangBufferText) {
            Debug.Assert(GrowingSpanDatas.Count == mappings.Length);

            List<object> sourceSpans = new List<object>(mappings.Length);
            int langIndex = 0; // last processed position in language buffer

            for (int i = 0; i < GrowingSpanDatas.Count; i++) {
                if (!GrowingSpanDatas[i].IsDisconnected) {
                    ProjectionMapping mapping = mappings[i];
                    Span inertSpan = Span.FromBounds(langIndex, mapping.ProjectionStart);

                    if (!inertSpan.IsEmpty) {
                        // Gather inert text between spans
                        sourceSpans.Add(fullLangBufferText.Substring(inertSpan.Start, inertSpan.Length));
                    }

                    sourceSpans.Add(GrowingSpanDatas[i].TrackingSpan);

                    langIndex = mapping.ProjectionStart + mapping.Length;
                }
            }

            // Add the final inert text after the last span
            Span lastInertSpan = Span.FromBounds(langIndex, fullLangBufferText.Length);

            if (!lastInertSpan.IsEmpty) {
                sourceSpans.Add(fullLangBufferText.Substring(lastInertSpan.Start, lastInertSpan.Length));
            }

            return sourceSpans;
        }

        /// <summary>
        /// Sets the text and spans in the language buffer based on a list of mappings
        /// </summary>
        private void UpdateTextBuffer(ProjectionMapping[] mappings, string fullLangBufferText) {
            List<object> sourceSpans = CreateSourceSpans(mappings, fullLangBufferText);
            EditOptions editOptions = ProjectionBuffer.GetAppropriateChangeEditOptions(LangBuffer.CurrentSnapshot.GetSourceSpans(), sourceSpans);

            LangBuffer.ReplaceSpans(0, LangBuffer.CurrentSnapshot.SpanCount, sourceSpans, editOptions, this);
        }

        /// <summary>
        /// Updates the projection span for each GrowingSpanData (ignoring disconnected spans).
        /// The language buffer's text must already be updated.
        /// </summary>
        private void UpdateProjectionTrackingSpans(ProjectionMapping[] mappings) {
            Debug.Assert(GrowingSpanDatas.Count == mappings.Length);

            for (int i = 0; i < GrowingSpanDatas.Count; i++) {
                if (!GrowingSpanDatas[i].IsDisconnected) {
                    ProjectionMapping mapping = mappings[i];
                    Span projectionSpan = new Span(mapping.ProjectionStart, mapping.Length);
                    SnapshotSpan snapSpan = new SnapshotSpan(LangBuffer.CurrentSnapshot, projectionSpan);

                    Debug.Assert(GrowingSpanDatas[i].ProjectionTrackingSpan == null);
                    GrowingSpanDatas[i].SetProjectionTrackingSpan(snapSpan);
                }
            }
        }

        private bool RemoveDisconnectedGrowingSpans() {
            Debug.Assert(GrowingSpanDatas.Count == _projectionMappingExtraData.Count);

            bool foundDisconnectedSpan = false;

            for (int i = GrowingSpanDatas.Count - 1; i >= 0; i--) {
                if (GrowingSpanDatas[i].IsDisconnected) {
                    GrowingSpanDatas.RemoveAt(i);
                    _projectionMappingExtraData.RemoveAt(i);

                    foundDisconnectedSpan = true;
                }
            }

            return foundDisconnectedSpan;
        }

        private void SetMappingsHelper(ProjectionMapping[] mappings, string fullLangBufferText) {
            ViewManager.RemoveLanguageBuffer(this);

            // Don't need to keep checking this for null
            mappings = mappings ?? new ProjectionMapping[0];

            InitializeGrowingSpans(mappings);

            // Ensure that the spans added above have been processed (like detecting overlap)
            ViewManager.Tracker.EnsureTrackingPoints();

            UpdateTextBuffer(mappings, fullLangBufferText);

            UpdateProjectionTrackingSpans(mappings);

            RemoveDisconnectedGrowingSpans();

            // hook the language buffer back up to the view buffer.
            ViewManager.AddLanguageBuffer(this);
        }

        public void RemoveDisconnectedMappings() {
            if (RemoveDisconnectedGrowingSpans()) {
                ProjectionMapping[] mappings = CreateProjectionMappingsFromSpanData();
                SetMappings(mappings);
            }
        }

        /// <summary>
        /// This can recreate the original mappings array that was passed to SetMappingsHelper
        /// based on the spans in GrowingSpanDatas.
        /// </summary>
        private ProjectionMapping[] CreateProjectionMappingsFromSpanData() {
            List<ProjectionMapping> newMappings = new List<ProjectionMapping>();

            for (int i = 0; i < GrowingSpanDatas.Count; i++) {
                GrowingSpanData spanData = GrowingSpanDatas[i];
                Debug.Assert(!spanData.IsDisconnected && spanData.ProjectionTrackingSpan != null);

                // Only use valid spans
                if (!spanData.IsDisconnected && spanData.ProjectionTrackingSpan != null) {
                    ProjectionMapping mapping = new ProjectionMapping(spanData.TrackingSpan.GetStartPoint(DiskBuffer.CurrentSnapshot),
                                                        spanData.ProjectionTrackingSpan.GetStartPoint(LangBuffer.CurrentSnapshot),
                                                        spanData.LatestSpan.Length,
                                                        _projectionMappingExtraData[i].trailingInclusion);

                    mapping.Properties.AddProperty("ExtraData", _projectionMappingExtraData[i].UserData);
                    newMappings.Add(mapping);
                }
            }

            return newMappings.ToArray();
        }

        public void RemoveSpans() {
            ViewManager.Tracker.RemoveBuffer(LangBuffer);
        }

        public TextRangeCollection<ProjectionMapping> Mappings {
            get {
                IProjectionSnapshot projectionSnapshot = LangBuffer.CurrentSnapshot;
                int mappingIndex = 0;
                int accumulatedLength = 0;
                List<ProjectionMapping> mappings = new List<ProjectionMapping>();

                ReadOnlyCollection<SnapshotSpan> sourceSpans = projectionSnapshot.GetSourceSpans();

                foreach (SnapshotSpan sourceSpan in sourceSpans) {
                    if (sourceSpan.Snapshot.TextBuffer == DiskBuffer) {
                        var mapping = new ProjectionMapping(sourceSpan.Start,
                                                            accumulatedLength,
                                                            sourceSpan.Length,
                                                            _projectionMappingExtraData[mappingIndex].trailingInclusion);

                        mapping.Properties.AddProperty("ExtraData", _projectionMappingExtraData[mappingIndex].UserData);
                        mappings.Add(mapping);
                        mappingIndex++;
                    }

                    accumulatedLength += sourceSpan.Length;
                }

                return new TextRangeCollection<ProjectionMapping>(mappings);
            }
        }

        public ProjectionMapping GetMappingAt(int position) {
            IProjectionSnapshot projectionSnapshot = LangBuffer.CurrentSnapshot;
            int mappingIndex = 0;
            int accumulatedLength = 0;
            ProjectionMapping foundMapping = null;

            ReadOnlyCollection<SnapshotSpan> sourceSpans = projectionSnapshot.GetSourceSpans();

            foreach (SnapshotSpan sourceSpan in sourceSpans) {
                if (sourceSpan.Snapshot.TextBuffer == DiskBuffer) {
                    // We can't short-circuit as the spans in the projection buffer
                    //   aren't in the same order as their location in the disk buffer.
                    //   However, we can prevent nearly all ProjectionMapping creations
                    //   by only creating ones which have the potential to contain position.
                    if ((sourceSpan.Start <= position) && (position <= sourceSpan.End)) {
                        ProjectionMapping mapping = new ProjectionMapping(
                                                            sourceSpan.Start,
                                                            accumulatedLength,
                                                            sourceSpan.Length,
                                                            _projectionMappingExtraData[mappingIndex].trailingInclusion);

                        mapping.Properties.AddProperty("ExtraData", _projectionMappingExtraData[mappingIndex].UserData);

                        // Let the mapping determine whether it contains this position
                        if (mapping.Contains(position)) {
                            foundMapping = mapping;
                            break;
                        }
                    }

                    mappingIndex++;
                }

                accumulatedLength += sourceSpan.Length;
            }

            return foundMapping;
        }

        #endregion
    }
}
