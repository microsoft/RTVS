// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using System.Linq;
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
        private const string _projectionContentTypeName = "projection";

        private readonly ITextBuffer _diskBuffer;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        public ProjectionBufferManager(ITextBuffer diskBuffer,
                                       IProjectionBufferFactoryService projectionBufferFactoryService,
                                       IContentTypeRegistryService contentTypeRegistryService,
                                       string secondaryContentTypeName) {
            _diskBuffer = diskBuffer;
            _contentTypeRegistryService = contentTypeRegistryService;

            var contentType = _contentTypeRegistryService.GetContentType(_projectionContentTypeName);
            ViewBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.None, contentType);

            contentType = _contentTypeRegistryService.GetContentType(secondaryContentTypeName);
            ContainedLanguageBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);

            ServiceManager.AddService<IProjectionBufferManager>(this, _diskBuffer);
        }

        public static IProjectionBufferManager FromTextBuffer(ITextBuffer textBuffer) {
            return ServiceManager.GetService<IProjectionBufferManager>(textBuffer);
        }

        #region IProjectionBufferManager
        //  Graph:
        //      View Buffer [ContentType = RMD Projection]
        //        |      \
        //        |    Secondary [ContentType = R]
        //        |      /
        //       Disk Buffer [ContentType = RMD]

        public IProjectionBuffer ViewBuffer { get; }

        public IProjectionBuffer ContainedLanguageBuffer { get; }

        public void SetProjectionMappings(string secondaryContent, IReadOnlyList<ProjectionMapping> mappings) {
            mappings = mappings ?? new List<ProjectionMapping>();

            var secondarySpans = CreateSecondarySpans(secondaryContent, mappings);
            ContainedLanguageBuffer.ReplaceSpans(0, ContainedLanguageBuffer.CurrentSnapshot.SpanCount, secondarySpans, EditOptions.DefaultMinimalChange, this);

            // Update primary (view) buffer projected spans. View buffer spans are all tracking spans:
            // they either come from primary content or secondary content. Inert spans do not participate.
            var viewSpans = CreateViewSpans(mappings);
            ViewBuffer.ReplaceSpans(0, ViewBuffer.CurrentSnapshot.SpanCount, viewSpans, EditOptions.DefaultMinimalChange, this);
        }

        public void Dispose() {
            ServiceManager.RemoveService<IProjectionBufferManager>(_diskBuffer);
        }
        #endregion

        private List<object> CreateSecondarySpans(string secondaryText, IReadOnlyList<ProjectionMapping> mappings) { 
            var spans = new List<object>(mappings.Count);

            int secondaryIndex = 0;
            Span span;

            for (int i = 0; i < mappings.Count; i++) {
                ProjectionMapping mapping = mappings[i];
                if (mapping.Length > 0) {
                    span = Span.FromBounds(secondaryIndex, mapping.ProjectionRange.Start);
                    if (!span.IsEmpty) {
                        spans.Add(secondaryText.Substring(span.Start, span.Length)); // inert
                    }
                    span = new Span(mapping.SourceStart, mapping.Length);
                    // Active span comes from the disk buffer
                    spans.Add(_diskBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive)); // active
                    secondaryIndex = mapping.ProjectionRange.End;
                }
            }

            // Add the final inert text after the last span
            span = Span.FromBounds(secondaryIndex, secondaryText.Length);
            if (!span.IsEmpty) {
                spans.Add(secondaryText.Substring(span.Start, span.Length)); // inert
            }
            return spans;
        }

        private List<object> CreateViewSpans(IReadOnlyList<ProjectionMapping> mappings) {
            var spans = new List<object>(mappings.Count);
            var diskSnapshot = _diskBuffer.CurrentSnapshot;
            int primaryIndex = 0;
            Span span;

            for (int i = 0; i < mappings.Count; i++) {
                ProjectionMapping mapping = mappings[i];
                if (mapping.Length > 0) {
                    span = Span.FromBounds(primaryIndex, mapping.SourceStart);
                    spans.Add(diskSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive)); // Markdown

                    span = new Span(mapping.ProjectionStart, mapping.Length);
                    spans.Add(ContainedLanguageBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive)); // R

                    primaryIndex = mapping.SourceRange.End;
                }
            }
            // Add the final inert text after the last span
            span = Span.FromBounds(primaryIndex, diskSnapshot.Length);
            if (!span.IsEmpty) {
                spans.Add(diskSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive)); // Markdown
            }
            return spans;
        }
    }
}
