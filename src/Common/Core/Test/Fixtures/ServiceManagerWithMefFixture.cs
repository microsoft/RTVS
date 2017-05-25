// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.Common.Core.Test.Fixtures {
    public abstract class ServiceManagerWithMefFixture : ServiceManagerFixture {
        private readonly Lazy<ComposablePartCatalog> _catalogLazy;

        protected ServiceManagerWithMefFixture() {
            _catalogLazy = new Lazy<ComposablePartCatalog>(() => {
                var catalog = CatalogFactory.CreateAssembliesCatalog(GetAssemblyNames());
                return FilterCatalog(catalog);
            });
        }

        protected virtual ComposablePartCatalog FilterCatalog(ComposablePartCatalog catalog) => catalog;

        protected abstract IEnumerable<string> GetAssemblyNames();

        protected override TestServiceManager CreateFixture() {
            return new TestServiceManagerWithMef(_catalogLazy.Value, SetupServices)
                .AddMef()
                .AddLog();
        }

        private sealed class TestServiceManagerWithMef : TestServiceManager {
            private readonly CompositionContainer _compositionContainer;
            private readonly DisposeToken _disposeToken = DisposeToken.Create< TestServiceManagerWithMef>();

            public TestServiceManagerWithMef(ComposablePartCatalog catalog, Action<IServiceManager, ITestInput> addServices) : base(addServices) {
                _compositionContainer = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);
            }

            public TestServiceManagerWithMef AddMef() {
                var batch = new CompositionBatch()
                    .AddValue<ICoreShell>(new TestCoreShell(this));

                _compositionContainer.Compose(batch);

                AddService(new TestCompositionCatalog(_compositionContainer));
                AddService<ExportProvider>(_compositionContainer);
                AddService<ICompositionService>(_compositionContainer);

                return this;
            }

            public override Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
                if (!_disposeToken.TryMarkDisposed()) {
                    return Task.CompletedTask;
                }
                _compositionContainer.Dispose();
                return base.DisposeAsync(result, messageBus);
            }

            public override T GetService<T>(Type type = null) {
                if (_disposeToken.IsDisposed) {
                    return null;
                }
                return base.GetService<T>(type) ?? _compositionContainer.GetExportedValueOrDefault<T>();
            }
        }
    }
}