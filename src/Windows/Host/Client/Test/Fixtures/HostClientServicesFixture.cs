// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Platform.Interpreters;
using Microsoft.R.Platform.IO;
using Microsoft.R.Platform.OS;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    public class HostClientServicesFixture : ServiceManagerFixture {
        protected override void SetupServices(IServiceManager serviceManager, ITestInput testInput) {
            base.SetupServices(serviceManager, testInput);
            serviceManager
                .AddWindowsHostClientServices()
                .AddService<IFileSystem, WindowsFileSystem>()
                .AddService<IRegistry, RegistryImpl>()
                .AddService<IProcessServices, WindowsProcessServices>()
                .AddService<IRInstallationService, RInstallation>();
        }
    }
}