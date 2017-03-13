// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Common.Core.Services;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    internal sealed class TestServiceManager: ServiceManager {
        private readonly ExportProvider _exportProvider;

        public TestServiceManager(ExportProvider exportProvider) {
            _exportProvider = exportProvider;
        }

        #region IServiceContainer
        public override T GetService<T>(Type type = null) {
            // First try internal services
            var service = base.GetService<T>(type);
            return service ?? _exportProvider.GetExportedValueOrDefault<T>();
        }
        #endregion
    }
}
