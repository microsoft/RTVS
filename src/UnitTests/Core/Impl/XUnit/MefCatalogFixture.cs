// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class MefCatalogFixture : IMethodFixtureFactory<IExportProvider>, IDisposable {
        private readonly Lazy<ComposablePartCatalog> _catalogLazy;
        private readonly ConcurrentQueue<IDisposable> _exportProviders = new ConcurrentQueue<IDisposable>();

        protected MefCatalogFixture() {
            _catalogLazy = new Lazy<ComposablePartCatalog>(CreateCatalog);
        }

        protected abstract ComposablePartCatalog CreateCatalog();

        protected CompositionContainer CreateContainer() => new CompositionContainer(_catalogLazy.Value, CompositionOptions.DisableSilentRejection);

        public IExportProvider Dummy { get; } = new NullExportProvider();

        void IDisposable.Dispose() {
            IDisposable exportProvider;
            while (_exportProviders.TryDequeue(out exportProvider)) {
                exportProvider.Dispose();
            }
        }

        private class NullExportProvider : IExportProvider {
            public Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus)
                => MethodFixtureBase.DefaultInitializeTask;

            public Task DisposeAsync(RunSummary result, IMessageBus messageBus) => Task.CompletedTask;
            public T GetExportedValue<T>() where T : class => default(T);
            public T GetExportedValue<T>(string metadataKey, params object[] metadataValues) where T : class => default(T);
            public IEnumerable<Lazy<T>> GetExports<T>() where T : class => null;
            public IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>() where T : class => null;
        }

        protected class TestExportProvider : IExportProvider {
            private readonly IDisposable _disposable;
            protected CompositionContainer CompositionContainer { get; }

            public TestExportProvider(CompositionContainer compositionContainer) {
                CompositionContainer = compositionContainer;
                _disposable = Disposable.Create(CompositionContainer);
            }

            public virtual Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
                return MethodFixtureBase.DefaultInitializeTask;
            }

            public Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
                _disposable.Dispose();
                return Task.CompletedTask;
            }

            public T GetExportedValue<T>() where T: class {
                if(typeof(T) == typeof(ICompositionService)) {
                    // TODO: remove when editor no longer uses SatisfyImportsOnce.
                    return CompositionContainer as T;
                }
                return CompositionContainer.GetExportedValue<T>();
            }

            public T GetExportedValue<T>(string metadataKey, params object[] metadataValues) where T : class {
                var exports = CompositionContainer.GetExports<T, IDictionary<string, object>>();

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

            public IEnumerable<Lazy<T>> GetExports<T>() where T : class {
                return CompositionContainer.GetExports<T>();
            }

            public IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>() where T : class {
                return CompositionContainer.GetExports<T, TMetadataView>();
            }
        }
    }
}