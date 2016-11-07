// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class MefCatalogFixture : IDisposable {
        private readonly Lazy<ComposablePartCatalog> _catalogLazy;
        private readonly ConcurrentQueue<IDisposable> _exportProviders = new ConcurrentQueue<IDisposable>();

        protected MefCatalogFixture() {
            _catalogLazy = new Lazy<ComposablePartCatalog>(CreateCatalog);
        }

        protected abstract ComposablePartCatalog CreateCatalog();

        protected virtual void AddValues(CompositionContainer container, string testName) {}

        public IExportProvider CreateExportProvider([CallerMemberName] string testName = null) {
            var container = new CompositionContainer(_catalogLazy.Value, CompositionOptions.DisableSilentRejection);
            AddValues(container, testName);
            var exportProvider = new TestExportProvider(container);
            _exportProviders.Enqueue(exportProvider);
            return exportProvider;
        }

        public IExportProvider CreateExportProvider(CompositionBatch additionalValues, [CallerMemberName] string testName = null) {
            var container = new CompositionContainer(_catalogLazy.Value, CompositionOptions.DisableSilentRejection);
            AddValues(container, testName);
            container.Compose(additionalValues);
            var exportProvider = new TestExportProvider(container);
            _exportProviders.Enqueue(exportProvider);
            return exportProvider;
        }

        void IDisposable.Dispose() {
            IDisposable exportProvider;
            while (_exportProviders.TryDequeue(out exportProvider)) {
                exportProvider.Dispose();
            }
        }

        private class TestExportProvider : IExportProvider {
            private readonly DisposableBag _disposableBag;
            private readonly CompositionContainer _compositionContainer;

            public TestExportProvider(CompositionContainer compositionContainer) {
                _compositionContainer = compositionContainer;
                _disposableBag = DisposableBag.Create<TestExportProvider>()
                    .Add(_compositionContainer);
            }

            public void Dispose() => _disposableBag.TryDispose();
            public T GetExportedValue<T>() {
                _disposableBag.ThrowIfDisposed();
                return _compositionContainer.GetExportedValue<T>();
            }

            public T GetExportedValue<T>(string metadataKey, params object[] metadataValues) {
                _disposableBag.ThrowIfDisposed();
                var exports = _compositionContainer.GetExports<T, IDictionary<string, object>>();

                return exports.Single(e => {
                    object value;
                    if (!e.Metadata.TryGetValue(metadataKey, out value)) {
                        return false;
                    }

                    var enumerable = value as IEnumerable<object>;
                    if (enumerable == null) {
                        return metadataValues.Length == 1 && Equals(value, metadataValues[0]);
                    }

                    return metadataValues.Intersect(enumerable).Any();
                }).Value;
            }

            public IEnumerable<Lazy<T>> GetExports<T>() {
                _disposableBag.ThrowIfDisposed();
                return _compositionContainer.GetExports<T>();
            }

            public IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>() {
                _disposableBag.ThrowIfDisposed();
                return _compositionContainer.GetExports<T, TMetadataView>();
            }
        }
    }
}