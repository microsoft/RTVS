// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Core.Composition;

namespace Microsoft.Languages.Editor.Composition {
    /// <summary>
    /// Retrieves all specified imports exported for a particular content type.
    /// Enumerates base types as well, so if request comes for, say, ASP.NET
    /// content type, also objects registered for HTML will be retrieved.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContentTypeImportComposer<T> : ImportComposer<T, IContentTypeMetadata> where T : class {
        [Import]
        IContentTypeRegistryService _contentTypeRegistryService { get; set; }

        public ContentTypeImportComposer(ICompositionService cs)
            : base(cs) {
            cs.SatisfyImportsOnce(this);
        }

        public ICollection<T> GetAll(IContentType contentType) {
            // Add imports for all base content types as well since, for example,
            // commands defined for HTML also apply to ASP or ASPX.
            var imports = new List<T>();

            imports.AddRange(base.GetAll(contentType.TypeName));

            var baseTypes = contentType.BaseTypes;
            foreach (var baseType in baseTypes)
                imports.AddRange(GetAll(baseType.TypeName));

            return imports;
        }

        public override ICollection<T> GetAll(string contentTypeName) {
            if (contentTypeName[0] == '*')
                return base.GetAll(contentTypeName);

            var contentType = _contentTypeRegistryService.GetContentType(contentTypeName);
            Debug.Assert(contentType != null);

            return this.GetAll(contentType);
        }

        public override T GetImport(string contentTypeName) {
            if (contentTypeName[0] == '*')
                return base.GetImport(contentTypeName);

            var contentType = _contentTypeRegistryService.GetContentType(contentTypeName);
            Debug.Assert(contentType != null);

            return this.GetImport(contentType);
        }

        public T GetImport(IContentType contentType) {
            var import = base.GetImport(contentType.TypeName);
            if (import != null)
                return import;

            var baseTypes = contentType.BaseTypes;
            foreach (var baseType in baseTypes) {
                import = GetImport(baseType.TypeName);
                if (import != null)
                    break;
            }

            return import;
        }

        public ICollection<Lazy<T, IContentTypeMetadata>> GetAllLazy(IContentType contentType) {
            // Add imports for all base content types as well since, for example,
            // commands defined for HTML also apply to ASP or ASPX.
            var imports = new List<Lazy<T, IContentTypeMetadata>>();

            imports.AddRange(base.GetAllLazy(contentType.TypeName));

            var baseTypes = contentType.BaseTypes;
            foreach (var baseType in baseTypes)
                imports.AddRange(GetAllLazy(baseType.TypeName));

            return imports;
        }

        public override ICollection<Lazy<T, IContentTypeMetadata>> GetAllLazy(string contentTypeName) {
            var contentType = _contentTypeRegistryService.GetContentType(contentTypeName);
            Debug.Assert(contentType != null);

            return GetAllLazy(contentType);
        }
    }
}
