// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Common.Core.Services;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public class TestServiceManager: ServiceManager {
        private readonly ExportProvider _exportProvider1;
        private readonly IExportProvider _exportProvider2;

        public TestServiceManager(ExportProvider exportProvider) {
            _exportProvider1 = exportProvider;
        }

        public TestServiceManager(IExportProvider exportProvider) {
            _exportProvider2 = exportProvider;
        }

        #region IServiceContainer
        public override T GetService<T>(Type type = null) {
            // First try internal services
            var service = base.GetService<T>(type);
            service = service ?? _exportProvider1?.GetExportedValueOrDefault<T>();
            try {
                service = service ?? _exportProvider2?.GetExportedValue<T>();
            } catch (ImportCardinalityMismatchException) { }
            return service;
        }
        #endregion
    }
}
