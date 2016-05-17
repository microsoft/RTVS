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

        public void SetProjectionMappings(ITextBuffer secondaryBuffer, string secondaryContent, IReadOnlyList<ProjectionMapping> mappings) {
            mappings = mappings ?? new List<ProjectionMapping>();
            var secondarySpans = CreateProjectionSpans(secondaryBuffer, secondaryContent, mappings);

            var editOptions = ProjectionBufferEditOptions.GetAppropriateChangeEditOptions(PrimaryProjectionBuffer.CurrentSnapshot.GetSourceSpans(), secondarySpans);
            SecondaryProjectionBuffer.ReplaceSpans(0, SecondaryProjectionBuffer.CurrentSnapshot.SpanCount, secondarySpans, editOptions, this);

            // Update primary (view) buffer projected spans. View buffer spans are all tracking spans:
            // they either come from primary content or secondary content. Inert spans do not participate.
            var primarySpans = CreatePrimarySpans(_diskBuffer, secondarySpans, mappings);
            PrimaryProjectionBuffer.ReplaceSpans(0, PrimaryProjectionBuffer.CurrentSnapshot.SpanCount, primarySpans, editOptions, this);
        }

        public void Dispose() {
            ServiceManager.RemoveService<IProjectionBufferManager>(_diskBuffer);
        }
        #endregion

        private List<object> CreateProjectionSpans(ITextBuffer secondaryBuffer, string secondaryText, IReadOnlyList<ProjectionMapping> mappings) { 
            var spans = new List<object>(mappings.Count);

            secondaryBuffer.Replace(new Span(0, secondaryBuffer.CurrentSnapshot.Length), secondaryText);
            int secondaryIndex = 0;
            Span span;

            for (int i = 0; i < mappings.Count; i++) {
                ProjectionMapping mapping = mappings[i];
                if (mapping.Length > 0) {
                    span = Span.FromBounds(secondaryIndex, mapping.Length);
                    spans.Add(secondaryText.Substring(span.End, mapping.Length)); // inert
                    spans.Add(secondaryBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive)); // active
                    secondaryIndex = mapping.ProjectionRange.End;
                }
            }

            // Add the final inert text after the last span
            span = Span.FromBounds(secondaryIndex, secondaryBuffer.CurrentSnapshot.Length);
            if (!span.IsEmpty) {
                spans.Add(secondaryText.Substring(span.Start, span.Length)); // inert
            }
            return spans;
        }

        private List<object> CreatePrimarySpans(ITextBuffer primaryBuffer, List<object> secondarySpans, IReadOnlyList<ProjectionMapping> mappings) {
            var spans = new List<object>(mappings.Count);
            int primaryIndex = 0;

            for (int i = 0; i < mappings.Count; i++) {
                ProjectionMapping mapping = mappings[i];
                if (mapping.Length > 0) {
                    var span = Span.FromBounds(primaryIndex, mapping.Length);
                    spans.Add(primaryBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive)); // active
                    spans.Add(secondarySpans[i]);
                    primaryIndex = mapping.ProjectionRange.End;
                }
            }
            return spans;
        }
    }
}
