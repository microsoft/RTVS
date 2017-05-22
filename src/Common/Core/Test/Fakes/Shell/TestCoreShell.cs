// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.UnitTests.Core.Threading;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestCoreShell : ICoreShell {
        public IServiceManager ServiceManager { get; }

        public TestCoreShell(IServiceManager serviceManager) {
             ServiceManager = serviceManager;
        }

        /// <summary>
        /// Creates an empty shell. Caller can add services as needed.
        /// </summary>
        public static TestCoreShell CreateEmpty() => new TestCoreShell(new ServiceManager());

        /// <summary>
        /// Creates shell with a set of basic functional services. 
        /// </summary>
        public static TestCoreShell CreateBasic() {
            var shell = new TestCoreShell(new ServiceManager());
            shell.AddBasicServices();
            return shell;
        }

        /// <summary>
        /// Creates shell with a set of basic services which are substitutes 
        /// </summary>
        public static TestCoreShell CreateSubstitute() {
            var shell = new TestCoreShell(new ServiceManager());
            shell.AddSubstiteServices();
            return shell;
        }

        private void AddSubstiteServices() {
            ServiceManager
                .AddService(this)
                .AddService(UIThreadHelper.Instance.MainThread)
                .AddService(Substitute.For<IActionLog>())
                .AddService(Substitute.For<ISecurityService>())
                .AddService(Substitute.For<ILoggingPermissions>())
                .AddService(Substitute.For<IFileSystem>())
                .AddService(Substitute.For<IRegistry>())
                .AddService(Substitute.For<IProcessServices>())
                .AddService(Substitute.For<ITaskService>())
                .AddService(Substitute.For<IUIService>())
                .AddService(Substitute.For<IPlatformServices>())
                .AddService(Substitute.For<IApplication>())
                .AddService(Substitute.For<IIdleTimeService>())
                .AddService(Substitute.For<IIdleTimeSource>());
        }

        private void AddBasicServices(IActionLog log = null
            , ILoggingPermissions loggingPermissions = null
            , IFileSystem fs = null
            , IRegistry registry = null
            , IProcessServices ps = null) {
            ServiceManager
                .AddService(this)
                .AddService(UIThreadHelper.Instance.MainThread)
                .AddService(log ?? Substitute.For<IActionLog>())
                .AddService(new SecurityServiceStub())
                .AddService(loggingPermissions ?? Substitute.For<ILoggingPermissions>())
                .AddService(fs ?? new WindowsFileSystem())
                .AddService(registry ?? new RegistryImpl())
                .AddService(ps ?? new ProcessServices())
                .AddService(new TestTaskService())
                .AddService(new TestUIServices(UIThreadHelper.Instance.ProgressDialog))
                .AddService(new TestImageService())
                .AddService(new TestPlatformServices())
                .AddService(new TestApplication())
                .AddService(new TestIdleTimeService());
        }

        public IServiceContainer Services => ServiceManager;
    }
}