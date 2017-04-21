// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Fixtures {
    public abstract class ServiceManagerWithMefFixture : ServiceManagerFixture {
        private readonly Lazy<ComposablePartCatalog> _catalogLazy;

        protected ServiceManagerWithMefFixture() {
            _catalogLazy = new Lazy<ComposablePartCatalog>(CreateCatalog);
        }

        protected abstract ComposablePartCatalog CreateCatalog();

        protected override TestServiceManager CreateFixture() {
            return new TestServiceManagerWithMef(_catalogLazy.Value, AddServices)
                .AddMef()
                .AddLog();
        }

        private sealed class TestServiceManagerWithMef : TestServiceManager {
            private readonly CompositionContainer _compositionContainer;

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

            public override T GetService<T>(Type type = null) {
                var service = base.GetService<T>(type);
                if (service != null) {
                    return service;
                }

                try {
                    return _compositionContainer.GetExportedValue<T>();
                } catch (ImportCardinalityMismatchException) {
                    return null;
                }
            }
        }
    }
}