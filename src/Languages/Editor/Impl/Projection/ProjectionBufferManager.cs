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
                                       string projectionContentTypeName) {
            _diskBuffer = diskBuffer;
            _contentTypeRegistryService = contentTypeRegistryService;

            var contentType = _contentTypeRegistryService.GetContentType(projectionContentTypeName);
            ProjectionBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);

            ServiceManager.AddService<IProjectionBufferManager>(this, _diskBuffer);
        }

        public static IProjectionBufferManager FromTextBuffer(ITextBuffer textBuffer) {
            return ServiceManager.GetService<IProjectionBufferManager>(textBuffer);
        }

        #region IProjectionBufferManager
        public IProjectionBuffer ProjectionBuffer { get; }

        public void SetTextAndMappings(string text, IReadOnlyList<ProjectionMapping> mappings) {
            mappings = mappings ?? new List<ProjectionMapping>();
            UpdateTextBuffer(mappings, text);
        }

        public void Dispose() {
            ServiceManager.RemoveService<IProjectionBufferManager>(_diskBuffer);
        }
        #endregion

        /// <summary>
        /// Sets the text and spans in the language buffer based on a list of mappings
        /// </summary>
        private void UpdateTextBuffer(IReadOnlyList<ProjectionMapping> mappings, string fullLangBufferText) {
            List<object> sourceSpans = CreateSourceSpans(mappings, fullLangBufferText);
            EditOptions editOptions = ProjectionBufferEditOptions.GetAppropriateChangeEditOptions(ProjectionBuffer.CurrentSnapshot.GetSourceSpans(), sourceSpans);

            ProjectionBuffer.ReplaceSpans(0, ProjectionBuffer.CurrentSnapshot.SpanCount, sourceSpans, editOptions, this);
        }

        /// <summary>
        /// Uses the current GrowingSpanDatas content to create a list of
        /// source spans and inert text spans. This should really only be called from UpdateTextBuffer.
        /// </summary>
        private List<object> CreateSourceSpans(IReadOnlyList<ProjectionMapping> mappings, string fullLangBufferText) {
            List<object> sourceSpans = new List<object>(mappings.Count);
            int langIndex = 0; // last processed position in language buffer

            for (int i = 0; i < mappings.Count; i++) {
                ProjectionMapping mapping = mappings[i];
                // Inert area is area that belongs to the primary (top-level) document
                Span inertSpan = Span.FromBounds(langIndex, mapping.ProjectionStart);
                if (!inertSpan.IsEmpty) {
                    // Gather inert text between spans
                    sourceSpans.Add(fullLangBufferText.Substring(inertSpan.Start, inertSpan.Length));
                }
                // Map contained language range
                Span languageSpan = new Span(mapping.ProjectionStart, mapping.ProjectionRange.Length);
                sourceSpans.Add(languageSpan);
                langIndex = mapping.ProjectionStart + mapping.Length;
            }

            // Add the final inert text after the last span
            Span lastInertSpan = Span.FromBounds(langIndex, fullLangBufferText.Length);
            if (!lastInertSpan.IsEmpty) {
                sourceSpans.Add(fullLangBufferText.Substring(lastInertSpan.Start, lastInertSpan.Length));
            }
            return sourceSpans;
        }
    }
}
