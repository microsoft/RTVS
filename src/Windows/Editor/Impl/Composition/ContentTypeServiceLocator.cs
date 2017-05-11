// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Assist in location of services specific to a particular file content type.
    /// </summary>
    /// 
    [Export(typeof(IContentTypeServiceLocator))]
    internal sealed class ContentTypeServiceLocator: IContentTypeServiceLocator {
        private readonly IContentTypeRegistryService _ctrs;
        private readonly ICompositionService _compositionService;

        [ImportingConstructor]
        public ContentTypeServiceLocator(ICoreShell coreShell) {
            _compositionService = coreShell.GetService<ICompositionService>();
            _ctrs = coreShell.GetService<IContentTypeRegistryService>();
        }

        /// <summary>
        /// Locates services for a content type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="contentType">Content (file) type such as 'R' or 'Markdown'</param>
        /// <returns>Service instance, if any</returns>
        public T GetService<T>(string contentType) where T: class {
            var importComposer = new ContentTypeImportComposer<T>(_compositionService);
            return importComposer.GetImport(contentType);
        }

        /// <summary>
        /// Retrieves all services of a particular type available for the content type.
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="contentType">Content (file) type such as 'R' or 'Markdown'</param>
        /// <returns>Collection of service instances, if any</returns>
        public IEnumerable<T> GetAllServices<T>(string contentType) where T : class {
            var importComposer = new ContentTypeImportComposer<T>(_compositionService);
            return importComposer.GetAll(contentType);
        }

        /// <summary>
        /// Retrieves all services of a particular type available for the content type.
        /// Services are ordered in a way they should be accessed. This applies, for example,
        /// to command controller factories so controllers are called in a specific order.
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="contentType">Content (file) type such as 'R' or 'Markdown'</param>
        /// <returns>Collection of service instances, if any</returns>
        public IEnumerable<Lazy<T>> GetAllOrderedServices<T>(string contentType) where T : class {
            var ct = _ctrs.GetContentType(contentType);
            return ct != null ? ComponentLocatorForOrderedContentType<T>.ImportMany(_compositionService, ct) : Enumerable.Empty<Lazy<T>>();
        }
    }
}
