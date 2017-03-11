// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public partial class VsAppShell {
        private VsServiceManager _services;

        public IServiceContainer GlobalServices { get; private set; }

        private void ConfigureServices() {
            _services = new VsServiceManager(this);

            _services.AddService(_coreServices);
            _services.AddService(_settings);
            // TODO: add more
        }
    }
}
