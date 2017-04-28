// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Composition {
    public static class ComponentLocator<TComponent> where TComponent : class {
        public static TComponent Import(ICompositionService compositionService) {
            var importer = new SingleImporter();
            compositionService.SatisfyImportsOnce(importer);
            return importer.Import;
        }

        public static IEnumerable<Lazy<TComponent>> ImportMany(ICompositionService compositionService) {
            var importer = new ManyImporter();
            compositionService.SatisfyImportsOnce(importer);
            return importer.Imports;
        }

        private class SingleImporter {
            [Import]
            public TComponent Import { get; set; }
        }

        private class ManyImporter {
            [ImportMany]
            public IEnumerable<Lazy<TComponent>> Imports { get; set; }
        }
    }

    public static class ComponentLocatorWithMetadata<TComponent, TMetadata>
        where TComponent : class
        where TMetadata : class {
        public static IEnumerable<Lazy<TComponent, TMetadata>> ImportMany(ICompositionService compositionService) {
            var importer = new ManyImporter();
            compositionService.SatisfyImportsOnce(importer);
            return importer.Imports;
        }

        private class ManyImporter {
            [ImportMany]
            public IEnumerable<Lazy<TComponent, TMetadata>> Imports { get; set; }
        }
    }

    /// <summary>
    /// Allows using [Order] and [Name] attributes on exports and have them imported in the correct order.
    /// </summary>
    public static class ComponentLocatorWithOrdering<TComponent, TMetadata>
        where TComponent : class
        where TMetadata : IOrderable {
        public static IEnumerable<Lazy<TComponent, TMetadata>> ImportMany(ICompositionService compositionService) {
            var importer = new ManyImporter();
            compositionService.SatisfyImportsOnce(importer);
            return Orderer.Order(importer.Imports);
        }

        /// <summary>
        /// Reverses the order of imported items
        /// </summary>
        public static IEnumerable<Lazy<TComponent, TMetadata>> ReverseImportMany(ICompositionService compositionService) 
            => ImportMany(compositionService).Reverse();

        private class ManyImporter {
            [ImportMany]
            public IEnumerable<Lazy<TComponent, TMetadata>> Imports { get; set; }
        }
    }

    /// <summary>
    /// Assumes IOrderable for the metadata interface
    /// </summary>
    public static class ComponentLocatorWithOrdering<TComponent> where TComponent : class {
        public static IEnumerable<Lazy<TComponent, IOrderable>> ImportMany(ICompositionService compositionService) 
            => ComponentLocatorWithOrdering<TComponent, IOrderable>.ImportMany(compositionService);

        public static IEnumerable<Lazy<TComponent, IOrderable>> ReverseImportMany(ICompositionService compositionService) 
            => ComponentLocatorWithOrdering<TComponent, IOrderable>.ReverseImportMany(compositionService);
    }

    /// <summary>
    /// Locates components by content type
    /// </summary>
    public static class ComponentLocatorForContentType<TComponent, TMetadata>
        where TComponent : class
        where TMetadata : class, IComponentContentTypes {
        /// <summary>
        /// Locates all components exported with a given content type or with any of the content type base types
        /// </summary>
        public static IEnumerable<Lazy<TComponent, TMetadata>> ImportMany(ICompositionCatalog catalog, string contentTypeName) {
            var lazy = catalog.ExportProvider.GetExport<IContentTypeRegistryService>();
            Debug.Assert(lazy != null);

            var contentTypeRegistry = lazy.Value;
            var contentType = contentTypeRegistry.GetContentType(contentTypeName);

            return ImportMany(catalog.CompositionService, contentType);
        }

        /// <summary>
        /// Locates all components exported with a given content type or with any of the content type base types
        /// </summary>

        public static IEnumerable<Lazy<TComponent, TMetadata>> ImportMany(ICompositionService compositionService, IContentType contentType) {
            var components = ComponentLocatorWithMetadata<TComponent, TMetadata>.ImportMany(compositionService);
            return FilterByContentType(contentType, components);
        }

        //// The resultant enumerable has the more specific content type matches before less specific ones
        public static IEnumerable<Lazy<TComponent, TMetadata>> FilterByContentType(IContentType contentType, IEnumerable<Lazy<TComponent, TMetadata>> components) {
            GetAllContentTypes(contentType, out List<IContentType>  allContentTypes, out List<string>  allContentTypeNames);

            foreach (var curContentType in allContentTypeNames) {
                foreach (var pair in components) {
                    if (pair.Metadata.ContentTypes != null) {
                        foreach (var componentContentType in pair.Metadata.ContentTypes) {
                            if (componentContentType.Equals(curContentType, StringComparison.OrdinalIgnoreCase)) {
                                yield return pair;
                            }
                        }
                    }
                }
            }
        }

        //// The resultant enumerable has the more specific content type matches before less specific ones
        public static IEnumerable<Lazy<TComponent, TMetadata>> FilterByContentTypeExact(IContentType contentType, IEnumerable<Lazy<TComponent, TMetadata>> components) {
            foreach (var pair in components) {
                if (pair.Metadata.ContentTypes != null) {
                    foreach (var componentContentType in pair.Metadata.ContentTypes) {
                        if (componentContentType.Equals(contentType.TypeName, StringComparison.OrdinalIgnoreCase)) {
                            yield return pair;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given content type provides collection that includes this type and all its base types
        /// </summary>
        private static void GetAllContentTypes(IContentType contentType, out List<IContentType> allContentTypes, out List<string> allContentTypeNames) {
            allContentTypes = new List<IContentType>();
            allContentTypeNames = new List<string>();

            allContentTypes.Add(contentType);

            // Add all base types and all their base types
            for (var i = 0; i < allContentTypes.Count; i++) {
                var curContentType = allContentTypes[i];

                allContentTypeNames.Add(curContentType.TypeName);
                foreach (var baseContentType in curContentType.BaseTypes) {
                    if (!allContentTypeNames.Contains(baseContentType.TypeName)) {
                        allContentTypes.Add(baseContentType);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Imports components that supply Order, Name, and ContentType as metadata.
    /// Maintains specified order within the content type.
    /// </summary>
    public static class ComponentLocatorForOrderedContentType<TComponent> where TComponent : class {
        public static IEnumerable<Lazy<TComponent>> ImportMany(ICompositionService compositionService, IContentType contentType) {
            var components = ComponentLocatorForContentType<TComponent, IOrderedComponentContentTypes>.ImportMany(compositionService, contentType);
            return Orderer.Order(components);
        }

        /// <summary>
        /// Locates first component withing ordered components of a particular content type.
        /// </summary>
        public static TComponent FindFirstOrderedComponent(ICompositionService compositionService, IContentType contentType) {
            var components = ImportMany(compositionService, contentType);
            return components.Select(pair => pair.Value).FirstOrDefault();
        }

        /// <summary>
        /// Locates first component withing ordered components of a particular content type.
        /// </summary>
        public static TComponent FindFirstOrderedComponent(ICompositionCatalog catalog, string contentTypeName) {
            var contentTypeRegistryService = catalog.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
            var contentType = contentTypeRegistryService.GetContentType(contentTypeName);
            return FindFirstOrderedComponent(catalog.CompositionService, contentType);
        }
    }
}
