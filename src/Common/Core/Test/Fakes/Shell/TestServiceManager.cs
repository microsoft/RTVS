// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.Common.Core.Services;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public class TestServiceManager : IServiceManager {
        private readonly ExportProvider _compositionContainer;
        private readonly IServiceManager _services;

        public TestServiceManager(CompositionContainer compositionContainer, IServiceManager services) {
            _compositionContainer = compositionContainer;
            _services = services;
            var catalog = new TestCompositionCatalog(compositionContainer);
            _services
                .AddService(catalog)
                .AddService(catalog.ExportProvider)
                .AddService(catalog.CompositionService);
        }

        public T GetService<T>(Type type = null) where T : class 
            => _services.GetService<T>(type) ?? _compositionContainer.GetExportedValue<T>();

        public IEnumerable<T> GetServices<T>() where T : class 
            => _services.GetServices<T>().Concat(_compositionContainer.GetExportedValues<T>());

        public IEnumerable<Type> AllServices => _services.AllServices;

        public event EventHandler<ServiceContainerEventArgs> ServiceAdded {
            add => _services.ServiceAdded += value;
            remove => _services.ServiceAdded -= value;
        }

        public event EventHandler<ServiceContainerEventArgs> ServiceRemoved {
            add => _services.ServiceRemoved += value;
            remove => _services.ServiceRemoved -= value;
        }

        public void Dispose() => _services.Dispose();

        public IServiceManager AddService<T>(T service, Type type = null) where T : class => _services.AddService(service, type);
        public IServiceManager AddService<T>(Func<T> factory = null) where T : class => _services.AddService(factory);
        public void RemoveService<T>() where T : class => _services.RemoveService<T>();
    }
}
