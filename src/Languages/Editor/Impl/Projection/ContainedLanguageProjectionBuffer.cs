// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// A text buffer for a secondary language, like CSS in HTML &lt;style> blocks,
    /// client script in &lt;script> blocks or server code in ASPX or PHP pages.
    /// </summary>
    public sealed class ContainedLanguageProjectionBuffer : ProjectionBufferBase {
        public event EventHandler<EventArgs> MappingsChanging;
        public event EventHandler<MappingsChangedEventArgs> MappingsChanged;

        internal ContainedLanguageProjectionManager LanguageProjectionManager { get; private set; }

        internal ContainedLanguageProjectionBuffer(
                                          ITextBuffer generatedTextBuffer,
                                          PrimaryLanguageProjectionManager viewProjectionManager,
                                          IContentTypeRegistryService contentTypeRegistryService,
                                         string projectionContentTypeName) {
            LanguageProjectionManager = new ContainedLanguageProjectionManager(generatedTextBuffer, viewProjectionManager, contentTypeRegistryService, projectionContentTypeName);
            ProjectionBuffer = LanguageProjectionManager.LanguageBuffer;
        }

        public void SetTextAndMappings(string text, ProjectionMapping[] mappings) {
            MappingsChanging?.Invoke(this, EventArgs.Empty);
            LanguageProjectionManager.SetTextAndMappings(text, mappings);
            MappingsChanged?.Invoke(this, new MappingsChangedEventArgs(text, mappings));
        }

        public void SetMappings(ProjectionMapping[] mappings) {
            MappingsChanging?.Invoke(this, EventArgs.Empty);
            LanguageProjectionManager.SetMappings(mappings);
            var bufferText = LanguageProjectionManager.LanguageBuffer.CurrentSnapshot.GetText();
            MappingsChanged?.Invoke(this, new MappingsChangedEventArgs(bufferText, mappings));
        }

        public void ResetMappings() {
            MappingsChanging?.Invoke(this, EventArgs.Empty);
            LanguageProjectionManager.ResetMappings();
            var bufferText = LanguageProjectionManager.LanguageBuffer.CurrentSnapshot.GetText();
            MappingsChanged?.Invoke(this, new MappingsChangedEventArgs(bufferText, LanguageProjectionManager.Mappings.ToArray()));
        }

        public TextRangeCollection<ProjectionMapping> Mappings => LanguageProjectionManager.Mappings;

        public ProjectionMapping GetMappingAt(int position) => LanguageProjectionManager.GetMappingAt(position);

        public void RemoveSpans()  => LanguageProjectionManager.RemoveSpans();
    }
}
