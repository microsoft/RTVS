// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Base class for simple contained language buffer generators
    /// that do not require additional decoration to the code
    /// like classic ASP, PHP, CSS and JScript or R inside RMD. 
    /// ASP.NET and Razor use different custom implementation.
    /// </summary>
    public abstract class BufferGenerator {
        protected ContainedLanguageProjectionBuffer ProjectionBuffer { get; private set; }
        protected ProjectionBufferManager ProjectionBufferManager { get; }
        protected LanguageBlockCollection LanguageBlocks { get; }
        protected IEditorDocument Document { get; }
        public IContentType ContentType { get; }

        protected BufferGenerator(IEditorDocument document, LanguageBlockCollection languageBlocks, IContentType contentType) {
            Document = document;
            LanguageBlocks = languageBlocks;
            ContentType = contentType;
            ProjectionBufferManager = ServiceManager.GetService<ProjectionBufferManager>(Document.TextBuffer);
        }

        public virtual void UpdateBuffer(bool forceRegeneration) {
            if (forceRegeneration || IsRegenerationNeeded())
                RegenerateBuffer();
        }

        internal virtual bool IsRegenerationNeeded() {
            // LanguageBlockHandler used to always force regen, so 
            // keep doing this for now unless a specific generator
            // overrides and implements the right logic.
            return true;
        }

        protected abstract void RegenerateBuffer();

        protected virtual bool EnsureProjectionBuffer() {
            if (ProjectionBufferManager == null) {
                return false;
            }

            if (ProjectionBuffer != null) {
                return true;
            }

            ProjectionBuffer = ProjectionBufferManager.GetProjectionBuffer(ContentType) as ContainedLanguageProjectionBuffer;
            return ProjectionBuffer != null;
        }
    }
}