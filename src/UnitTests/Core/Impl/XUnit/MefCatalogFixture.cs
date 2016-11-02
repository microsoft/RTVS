// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class MefCatalogFixture : IDisposable {
        private readonly Lazy<ComposablePartCatalog> _catalogLazy;
        private readonly ConcurrentQueue<CompositionContainer> _containers = new ConcurrentQueue<CompositionContainer>();

        protected MefCatalogFixture() {
            _catalogLazy = new Lazy<ComposablePartCatalog>(CreateCatalog);
        }

        protected abstract ComposablePartCatalog CreateCatalog();

        protected virtual void AddValues(CompositionContainer container, string testName) {}

        public IExportProvider CreateExportProvider([CallerMemberName] string testName = null) {
            var container = new CompositionContainer(_catalogLazy.Value, CompositionOptions.DisableSilentRejection);
            AddValues(container, testName);
            _containers.Enqueue(container);
            return new TestExportProvider(container);
        }

        public IExportProvider CreateExportProvider(CompositionBatch additionalValues, [CallerMemberName] string testName = null) {
            var container = new CompositionContainer(_catalogLazy.Value, CompositionOptions.DisableSilentRejection);
            AddValues(container, testName);
            container.Compose(additionalValues);
            _containers.Enqueue(container);
            return new TestExportProvider(container);
        }

        void IDisposable.Dispose() {
            CompositionContainer container;
            while (_containers.TryDequeue(out container)) {
                container.Dispose();
            }
        }

        private class TestExportProvider : IExportProvider {
            private readonly CompositionContainer _compositionContainer;

            public TestExportProvider(CompositionContainer compositionContainer) {
                _compositionContainer = compositionContainer;
            }

            public void Dispose() => _compositionContainer.Dispose();
            public T GetExportedValue<T>() => _compositionContainer.GetExportedValue<T>();
            public IEnumerable<Lazy<T>> GetExports<T>() => _compositionContainer.GetExports<T>();
            public IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>() => _compositionContainer.GetExports<T, TMetadataView>();
        }
    }
}