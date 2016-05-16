// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Manages the projection buffer for the primary language
    /// </summary>
    public sealed class ProjectionBufferManager : IProjectionBufferManager {
        private const string _inertContentTypeName = "inert";
        private readonly ITextBuffer _diskBuffer;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        public ProjectionBufferManager(ITextBuffer diskBuffer,
                                       IProjectionBufferFactoryService projectionBufferFactoryService,
                                       IContentTypeRegistryService contentTypeRegistryService,
                                       string primaryProjectionContentTypeName,
                                       string secondaryContentType) {
            _diskBuffer = diskBuffer;
            _contentTypeRegistryService = contentTypeRegistryService;

            var contentType = _contentTypeRegistryService.GetContentType(primaryProjectionContentTypeName);
            PrimaryProjectionBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);

            contentType = _contentTypeRegistryService.GetContentType(secondaryContentType);
            SecondaryProjectionBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);

            ServiceManager.AddService<IProjectionBufferManager>(this, _diskBuffer);
        }

        public static IProjectionBufferManager FromTextBuffer(ITextBuffer textBuffer) {
            return ServiceManager.GetService<IProjectionBufferManager>(textBuffer);
        }

        #region IProjectionBufferManager
        public IProjectionBuffer PrimaryProjectionBuffer { get; }

        public IProjectionBuffer SecondaryProjectionBuffer { get; }

        public void SetProjectionMappings(ITextBuffer primaryBuffer, ITextBuffer secondaryBuffer, string secondaryContent, IReadOnlyList<ProjectionMapping> mappings) {
            mappings = mappings ?? new List<ProjectionMapping>();
            List<object> primarySpans;
            List<object> secondarySpans;

            CreateSpans(primaryBuffer, secondaryBuffer, mappings, out primarySpans, out secondarySpans);

            var editOptions = ProjectionBufferEditOptions.GetAppropriateChangeEditOptions(PrimaryProjectionBuffer.CurrentSnapshot.GetSourceSpans(), primarySpans);
            PrimaryProjectionBuffer.ReplaceSpans(0, PrimaryProjectionBuffer.CurrentSnapshot.SpanCount, primarySpans, editOptions, this);

            editOptions = ProjectionBufferEditOptions.GetAppropriateChangeEditOptions(PrimaryProjectionBuffer.CurrentSnapshot.GetSourceSpans(), secondarySpans);
            SecondaryProjectionBuffer.ReplaceSpans(0, SecondaryProjectionBuffer.CurrentSnapshot.SpanCount, secondarySpans, editOptions, this);
        }

        public void Dispose() {
            ServiceManager.RemoveService<IProjectionBufferManager>(_diskBuffer);
        }
        #endregion

        /// <summary>
        /// Uses the current GrowingSpanDatas content to create a list of
        /// source spans and inert text spans. This should really only be called from UpdateTextBuffer.
        /// </summary>
        private void CreateSpans(ITextBuffer primaryBuffer, ITextBuffer secondaryBuffer,
                                 IReadOnlyList<ProjectionMapping> mappings, 
                                 out List<object> primarySpans, out List<object> secondarySpans) {
            primarySpans = new List<object>(mappings.Count);
            secondarySpans = new List<object>(mappings.Count);

            var primaryText = primaryBuffer.CurrentSnapshot.GetText();
            var secondaryText = secondaryBuffer.CurrentSnapshot.GetText();
            int primaryIndex = 0;
            int secondaryIndex = 0;
            Span primarySpan, secondarySpan;

            for (int i = 0; i < mappings.Count; i++) {
                ProjectionMapping mapping = mappings[i];
                if (mapping.Length > 0) {
                    // Inert area is area that belongs to the primary (top-level) document
                    // is active area for the secondary buffer and vise versa.
                    primarySpan = Span.FromBounds(primaryIndex, mapping.SourceStart);
                    primarySpans.Add(primaryBuffer.CurrentSnapshot.CreateTrackingSpan(primarySpan, SpanTrackingMode.EdgeInclusive)); // active
                    primarySpans.Add(primaryText.Substring(mapping.SourceRange.Start, mapping.Length)); // inert
                    primaryIndex = mapping.SourceRange.End;

                    secondarySpan = Span.FromBounds(secondaryIndex, mapping.Length);
                    secondarySpans.Add(secondaryText.Substring(secondarySpan.End, mapping.Length)); // inert
                    secondarySpans.Add(secondaryBuffer.CurrentSnapshot.CreateTrackingSpan(secondarySpan, SpanTrackingMode.EdgeInclusive)); // active
                    secondaryIndex = mapping.ProjectionRange.End;
                }
            }

            // Add the final inert text after the last span
            primarySpan = Span.FromBounds(primaryIndex, primaryBuffer.CurrentSnapshot.Length);
            if (!primarySpan.IsEmpty) {
                primarySpans.Add(primaryText.Substring(primarySpan.Start, primarySpan.Length)); // inert
            }

            secondarySpan = Span.FromBounds(secondaryIndex, secondaryBuffer.CurrentSnapshot.Length);
            if (!secondarySpan.IsEmpty) {
                secondarySpans.Add(secondaryText.Substring(secondarySpan.Start, secondarySpan.Length)); // inert
            }
        }
    }
}
