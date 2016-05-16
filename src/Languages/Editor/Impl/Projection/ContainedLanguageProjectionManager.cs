// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Manages the projection buffer for a specific contained language
    /// R inside R Markdown. The implementation is basic, files with
    /// large number of complex mappings like Razor (CSHTML) require much 
    /// more complex approach with caching and incremental projection updates.
    /// </summary>
    internal class ContainedLanguageProjectionManager : IProjectionEditResolver {
        private const string _inertContentTypeName = "inert";
        private readonly PrimaryLanguageProjectionManager _viewManager;
        private readonly ITextBuffer _textBuffer;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        public IProjectionBuffer LanguageBuffer { get; }

        public ContainedLanguageProjectionManager(
                                         ITextBuffer generatedTextBuffer, 
                                         PrimaryLanguageProjectionManager viewManager,
                                         IProjectionBufferFactoryService projectionBufferFactoryService,
                                         IContentTypeRegistryService contentTypeRegistryService,
                                         string projectionContentTypeName) {
            _textBuffer = generatedTextBuffer;
            _contentTypeRegistryService = contentTypeRegistryService;

            var contentType = _contentTypeRegistryService.GetContentType(projectionContentTypeName);
            LanguageBuffer = projectionBufferFactoryService.CreateProjectionBuffer(this, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);
            _viewManager = viewManager;
        }

        public void SetTextAndMappings(string text, ProjectionMapping[] mappings) {
            SetMappings(mappings, text);
        }

        public void SetMappings(ProjectionMapping[] mappings) {
            if (LanguageBuffer != null) {
                SetMappings(mappings, LanguageBuffer.CurrentSnapshot.GetText());
            }
        }

        public void ResetMappings() {
            if (LanguageBuffer != null) {
                // This is a bit lame, as we've already set the content type and the content/mappings
                //   of the projection buffer. However, C# depends on a call to their SetColorizer method
                //   from the env LanguageServiceClassifier to actually do any colorization. 
                //   This only happens when the LanguageServiceClassificationTagger.TryCreateTagger
                //   method succeeds, which can only happen when TextDocData.OnTextBufferInitialized
                //   has been completed. However, this isn't called until ServerContainedLanguageSupport.EnsureBufferCoordinator
                //   at which point we've already set the content/mappings. Fix this by forcing
                //   a call to TryCreateTagger by changing the content type instead of resetting the projections
                //   as we used to do as this is *slow* due to platform's diffing code.
                var inertContentType = _contentTypeRegistryService.GetContentType(_inertContentTypeName);
                var oldContentType = LanguageBuffer.ContentType;

                LanguageBuffer.ChangeContentType(inertContentType, null);
                LanguageBuffer.ChangeContentType(oldContentType, null);
            }
        }

        /// <summary>
        /// Uses the current GrowingSpanDatas content to create a list of
        /// source spans and inert text spans. This should really only be called from UpdateTextBuffer.
        /// </summary>
        private List<object> CreateSourceSpans(ProjectionMapping[] mappings, string fullLangBufferText) {
            List<object> sourceSpans = new List<object>(mappings.Length);
            int langIndex = 0; // last processed position in language buffer

            for (int i = 0; i < mappings.Length; i++) {
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

        /// <summary>
        /// Sets the text and spans in the language buffer based on a list of mappings
        /// </summary>
        private void UpdateTextBuffer(ProjectionMapping[] mappings, string fullLangBufferText) {
            List<object> sourceSpans = CreateSourceSpans(mappings, fullLangBufferText);
            EditOptions editOptions = ProjectionBuffer.GetAppropriateChangeEditOptions(LanguageBuffer.CurrentSnapshot.GetSourceSpans(), sourceSpans);

            LanguageBuffer.ReplaceSpans(0, LanguageBuffer.CurrentSnapshot.SpanCount, sourceSpans, editOptions, this);
        }

        private void SetMappings(ProjectionMapping[] mappings, string fullLangBufferText) {
            _viewManager.RemoveLanguageBuffer(this);
            // Don't need to keep checking this for null
            mappings = mappings ?? new ProjectionMapping[0];

            UpdateTextBuffer(mappings, fullLangBufferText);
            // hook the language buffer back up to the view buffer.
            _viewManager.AddLanguageBuffer(this);
        }

        public void RemoveSpans() {
            _viewManager.RemoveLanguageBuffer(this);
        }

        public TextRangeCollection<ProjectionMapping> Mappings {
            get {
                IProjectionSnapshot projectionSnapshot = LanguageBuffer.CurrentSnapshot;
                List<ProjectionMapping> mappings = new List<ProjectionMapping>();
                ReadOnlyCollection<SnapshotSpan> sourceSpans = projectionSnapshot.GetSourceSpans();
                int mappingIndex = 0;
                int accumulatedLength = 0;

                foreach (SnapshotSpan sourceSpan in sourceSpans) {
                    if (sourceSpan.Snapshot.TextBuffer == _textBuffer) {
                        var mapping = new ProjectionMapping(sourceSpan.Start,
                                                            accumulatedLength,
                                                            sourceSpan.Length);
                        mappings.Add(mapping);
                        mappingIndex++;
                    }
                    accumulatedLength += sourceSpan.Length;
                }
                return new TextRangeCollection<ProjectionMapping>(mappings);
            }
        }

        public ProjectionMapping GetMappingAt(int position) {
            IProjectionSnapshot projectionSnapshot = LanguageBuffer.CurrentSnapshot;
            ReadOnlyCollection<SnapshotSpan> sourceSpans = projectionSnapshot.GetSourceSpans();
            ProjectionMapping foundMapping = null;
            int mappingIndex = 0;
            int accumulatedLength = 0;

            foreach (SnapshotSpan sourceSpan in sourceSpans) {
                if (sourceSpan.Snapshot.TextBuffer == _textBuffer) {
                    // We can't short-circuit as the spans in the projection buffer
                    //   aren't in the same order as their location in the disk buffer.
                    //   However, we can prevent nearly all ProjectionMapping creations
                    //   by only creating ones which have the potential to contain position.
                    if ((sourceSpan.Start <= position) && (position <= sourceSpan.End)) {
                        ProjectionMapping mapping = new ProjectionMapping(sourceSpan.Start, accumulatedLength, sourceSpan.Length);
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
    }
}
