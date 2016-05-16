// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Manages projection buffers for languages present the top level (primary) document
    /// </summary>
    public class ProjectionBufferManager {
        public event EventHandler<ProjectionBufferCreatedEventArgs> LanguageBufferCreated;

        [Import]
        private IProjectionBufferFactoryService ProjectionBufferFactoryService { get; set; }

        [Import]
        private IFileExtensionRegistryService FileExtensionRegistryService { get; set; }

        /// <summary>
        /// Primary text buffer
        /// </summary>
        private ITextBuffer _diskBuffer;

        /// <summary>
        /// Map of content types to projection buffers (content types present in the document)
        /// </summary>
        private Dictionary<IContentType, LanguageProjectionBuffer> _languageBuffers = new Dictionary<IContentType, LanguageProjectionBuffer>();

        private ViewProjectionManager _viewProjectionManager;

        public ProjectionBufferManager(ITextBuffer diskBuffer) {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);

            _diskBuffer = diskBuffer;
            ServiceManager.AddService<ProjectionBufferManager>(this, diskBuffer);

            var document = ServiceManager.GetService<IEditorDocument>(diskBuffer);
            document.DocumentClosing += OnClosing;
        }

        void OnClosing(object sender, EventArgs args) {
            if (_viewProjectionManager != null) {
                _viewProjectionManager.Close();
                _viewProjectionManager = null;
            }

            ServiceManager.RemoveService<ProjectionBufferManager>(_diskBuffer);

            var document = (IEditorDocument)sender;
            document.DocumentClosing -= OnClosing;
        }

        public static ProjectionBufferManager FromTextBuffer(ITextBuffer textBuffer) {
            return ServiceManager.GetService<ProjectionBufferManager>(textBuffer);
        }

        public IEnumerable<LanguageProjectionBuffer> LanguageBuffers => _languageBuffers.Values;

        /// <summary>
        /// Retrieves language projection buffer for a given content type
        /// </summary>
        /// <param name="contentType">Content type</param>
        /// <returns>Projection buffer</returns>
        public LanguageProjectionBuffer GetProjectionBuffer(IContentType contentType) {
            LanguageProjectionBuffer projectionBuffer;
            if (!_languageBuffers.TryGetValue(contentType, out projectionBuffer)) {
                if (!contentType.IsOfType(HtmlContentTypeDefinition.HtmlContentType)) {
                    IProjectionBuffer viewProjectionBuffer = ViewProjectionBuffer;
                    projectionBuffer = new LanguageProjectionBuffer(_diskBuffer, contentType, _viewProjectionManager);

                    _languageBuffers.Add(contentType, projectionBuffer);
                    LanguageBufferCreated?.Invoke(this, new ProjectionBufferCreatedEventArgs(contentType, projectionBuffer));
                }
            }

            return projectionBuffer;
        }

        /// <summary>
        /// Retrieves language projection buffer for a given file extension
        /// </summary>
        /// <param name="fileExtension">File extension like .cs</param>
        /// <returns>Projection buffer</returns>
        public ProjectionBuffer GetProjectionBuffer(string fileExtension) {
            // Primarily used by Razor tooling support code in WebStack branch
            var contentType = FileExtensionRegistryService.GetContentTypeForExtension(fileExtension);
            if (contentType == null) {
                return null;
            }
            return GetProjectionBuffer(contentType);
        }

        public IProjectionBuffer ViewProjectionBuffer {
            get {
                if (_viewProjectionManager == null) {
                    _viewProjectionManager = new ViewProjectionManager(_diskBuffer, ProjectionBufferFactoryService);
                }
                return _viewProjectionManager.ViewBuffer;
            }
        }
    }
}
