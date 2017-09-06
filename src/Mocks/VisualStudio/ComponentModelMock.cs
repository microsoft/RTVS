// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Platform.Composition;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class ComponentModelMock : IComponentModel {
        private readonly ICompositionCatalog _catalog;

        public ComponentModelMock(ICompositionCatalog catalog) {
            _catalog = catalog;
        }

        public ComposablePartCatalog DefaultCatalog => _catalog as ComposablePartCatalog;
        public ICompositionService DefaultCompositionService => _catalog.CompositionService;
        public ExportProvider DefaultExportProvider => _catalog.ExportProvider;
        public ComposablePartCatalog GetCatalog(string catalogName) => _catalog as ComposablePartCatalog;
        public IEnumerable<T> GetExtensions<T>() where T : class => _catalog.ExportProvider.GetExportedValues<T>();
        public T GetService<T>() where T : class => _catalog.ExportProvider.GetExport<T>().Value;
    }
}
