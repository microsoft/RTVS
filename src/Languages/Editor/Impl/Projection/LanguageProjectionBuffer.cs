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
    public sealed class LanguageProjectionBuffer : ProjectionBuffer {
        public event EventHandler<EventArgs> MappingsChanging;
        public event EventHandler<MappingsChangedEventArgs> MappingsChanged;

        internal LanguageProjectionManager LanguageProjectionManager { get; private set; }

        internal LanguageProjectionBuffer(ITextBuffer buffer,
                                          IContentType contentType,
                                          ViewProjectionManager viewProjectionManager)
            : base(buffer) {
            LanguageProjectionManager = new LanguageProjectionManager(DiskBuffer, viewProjectionManager, contentType);
            IProjectionBuffer = LanguageProjectionManager.LangBuffer;
        }

        public void SetTextAndMappings(string text, ProjectionMapping[] mappings) {
            FireMappingsChanging();
            LanguageProjectionManager.SetTextAndMappings(text, mappings);
            FireMappingsChanged(text, mappings);
        }

        public void SetMappings(ProjectionMapping[] mappings) {
            FireMappingsChanging();
            LanguageProjectionManager.SetMappings(mappings);
            FireMappingsChanged(LanguageProjectionManager.LangBuffer.CurrentSnapshot.GetText(), mappings);
        }

        public void ResetMappings() {
            MappingsChanging?.Invoke(this, EventArgs.Empty);
            LanguageProjectionManager.ResetMappings();
            FireMappingsChanged(LanguageProjectionManager.LangBuffer.CurrentSnapshot.GetText(), LanguageProjectionManager.Mappings.ToArray());
        }

        public TextRangeCollection<ProjectionMapping> Mappings => LanguageProjectionManager.Mappings;

        public ProjectionMapping GetMappingAt(int position) => LanguageProjectionManager.GetMappingAt(position);

        public void RemoveSpans()  => LanguageProjectionManager.RemoveSpans();

        private void FireMappingsChanged(string bufferText, ProjectionMapping[] mappings) {
            if (MappingsChanged != null)
                MappingsChanged(this, new MappingsChangedEventArgs(bufferText, mappings));

            var contentTypeName = LanguageProjectionManager.LangBuffer.CurrentSnapshot.TextBuffer.ContentType.TypeName;
        }
    }
}
