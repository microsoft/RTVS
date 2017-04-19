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
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class TestMefCatalog : IDisposable {
        private readonly Lazy<ComposablePartCatalog> _catalogLazy;
        private readonly ConcurrentQueue<IDisposable> _exportProviders = new ConcurrentQueue<IDisposable>();

        protected TestMefCatalog() {
            _catalogLazy = new Lazy<ComposablePartCatalog>(CreateCatalog);
        }

        protected abstract ComposablePartCatalog CreateCatalog();
        public CompositionContainer CreateContainer() => new CompositionContainer(_catalogLazy.Value, CompositionOptions.DisableSilentRejection);

        void IDisposable.Dispose() {
            IDisposable exportProvider;
            while (_exportProviders.TryDequeue(out exportProvider)) {
                exportProvider.Dispose();
            }
        }

        protected class TestExportProvider : IExportProvider, IDisposable {
            private readonly IDisposable _disposable;
            protected CompositionContainer CompositionContainer { get; }

            public TestExportProvider(CompositionContainer compositionContainer) {
                CompositionContainer = compositionContainer;
                _disposable = Disposable.Create(CompositionContainer);
            }

            #region IDisposable
            protected virtual void Dispose(bool disposing) {
                _disposable.Dispose();
            }
            public void Dispose() { }
            #endregion

            #region IExportProvider
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
            #endregion
        }
    }
}