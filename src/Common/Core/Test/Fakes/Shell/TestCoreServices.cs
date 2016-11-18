// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Test.Telemetry;
using Microsoft.Common.Core.Threading;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public static class TestCoreServices {
        public static ICoreServices CreateSubstitute(IFileSystem fs = null, IRegistry registry = null, IProcessServices ps = null) {
            return new CoreServices(
                Substitute.For<IApplicationConstants>(),
                Substitute.For<ITelemetryService>(),
                null,
                Substitute.For<ISecurityService>(),
                Substitute.For<ITaskService>(),
                Substitute.For<ISettingsStorage>(),
                new Lazy<IMainThread>(() => new TestMainThread()),
                Substitute.For<IActionLog>(),
                fs ?? Substitute.For<IFileSystem>(),
                registry ?? Substitute.For<IRegistry>(),
                ps ?? Substitute.For<IProcessServices>());
        }

        public static ICoreServices CreateReal() {
            return new CoreServices(
                new TestAppConstants(),
                new TelemetryTestService(),
                null,
                Substitute.For<ISecurityService>(),
                new TestTaskService(),
                Substitute.For<ISettingsStorage>(),
                new Lazy<IMainThread>(() => new TestMainThread()),
                Substitute.For<IActionLog>(),
                new FileSystem(),
                new RegistryImpl(),
                new ProcessServices());
        }
    }
}
