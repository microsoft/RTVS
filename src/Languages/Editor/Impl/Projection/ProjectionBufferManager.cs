// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Manages projection buffers for languages present the top level (primary) document.
    /// </summary>
    public sealed class ProjectionBufferManager {
        public event EventHandler<ProjectionBufferCreatedEventArgs> LanguageBufferCreated;

        private readonly ITextBuffer _diskBuffer;
        private readonly Dictionary<IContentType, ContainedLanguageProjectionBuffer> 
            _containedLanguageBuffers = new Dictionary<IContentType, ContainedLanguageProjectionBuffer>();

        private PrimaryLanguageProjectionManager _primaryProjectionManager;

        /// <summary>
        /// Constructs projection buffer manager based on the disk buffer
        /// content. Disk buffer always represents top level or primary document.
        /// Contained language buffers are generated in-memory and do not have
        /// persistent representation on disk.
        /// </summary>
        public ProjectionBufferManager(ITextBuffer diskBuffer, 
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IContentTypeRegistryService contentTypeRegistryService,
            string projectionContentTypeName) {

            _diskBuffer = diskBuffer;
            _primaryProjectionManager = new PrimaryLanguageProjectionManager(_diskBuffer,
                projectionBufferFactoryService, contentTypeRegistryService.GetContentType(projectionContentTypeName));

            ServiceManager.AddService<ProjectionBufferManager>(this, diskBuffer);

            var document = ServiceManager.GetService<IEditorDocument>(diskBuffer);
            document.DocumentClosing += OnClosing;
        }

        void OnClosing(object sender, EventArgs args) {
            var document = (IEditorDocument)sender;
            document.DocumentClosing -= OnClosing;
            ServiceManager.RemoveService<ProjectionBufferManager>(_diskBuffer);
        }

        public static ProjectionBufferManager FromTextBuffer(ITextBuffer textBuffer) {
            return ServiceManager.GetService<ProjectionBufferManager>(textBuffer);
        }

        public IEnumerable<ContainedLanguageProjectionBuffer> LanguageBuffers => _containedLanguageBuffers.Values;

        /// <summary>
        /// Retrieves language projection buffer for a given content type
        /// </summary>
        /// <param name="contentType">Content type</param>
        /// <returns>Projection buffer</returns>
        public ContainedLanguageProjectionBuffer GetProjectionBuffer(IContentType contentType) {
            ContainedLanguageProjectionBuffer projectionBuffer;
            if (!_containedLanguageBuffers.TryGetValue(contentType, out projectionBuffer)) {
                if (!contentType.IsOfType(HtmlContentTypeDefinition.HtmlContentType)) {
                    projectionBuffer = new ContainedLanguageProjectionBuffer(_diskBuffer, contentType, _primaryProjectionManager);
                    _containedLanguageBuffers.Add(contentType, projectionBuffer);
                    LanguageBufferCreated?.Invoke(this, new ProjectionBufferCreatedEventArgs(contentType, projectionBuffer));
                }
            }

            return projectionBuffer;
        }

        public IProjectionBuffer PrimaryProjectionBuffer => _primaryProjectionManager.ViewBuffer;
    }
}
