// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestCoreShell : ICoreShell, IIdleTimeSource {
        public IServiceManager ServiceManager { get; }

        private TestCoreShell(IServiceManager serviceManager) {
             ServiceManager = serviceManager;
        }

        /// <summary>
        /// Creates an empty shell. Caller can add services as needed.
        /// </summary>
        public static TestCoreShell CreateEmpty() {
            return new TestCoreShell(new ServiceManager());
        }

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

        /// <summary>
        /// Creates test core shell with basic services and delegation
        /// to the supplied export provider for additional services.
        /// </summary>
        /// <param name="exportProvider"></param>
        public TestCoreShell(IExportProvider exportProvider) : this(new TestServiceManager(exportProvider)) {
            AddBasicServices();
        }

        public TestCoreShell(ICompositionCatalog catalog) : this(new TestServiceManager(catalog.ExportProvider)) {
            AddBasicServices();
            ServiceManager
                .AddService(catalog)
                .AddService(catalog.ExportProvider)
                .AddService(catalog.CompositionService);
        }

        private void AddSubstiteServices() {
            ServiceManager
                .AddService(Substitute.For<IMainThread>())
                .AddService(Substitute.For<IActionLog>())
                .AddService(Substitute.For<ISecurityService>())
                .AddService(Substitute.For<ILoggingPermissions>())
                .AddService(Substitute.For<IFileSystem>())
                .AddService(Substitute.For<IRegistry>())
                .AddService(Substitute.For<IProcessServices>())
                .AddService(Substitute.For<ITaskService>())
                .AddService(Substitute.For<IUIService>())
                .AddService(Substitute.For<IPlatformServices>());
        }

        private void AddBasicServices(IActionLog log = null
            , ILoggingPermissions loggingPermissions = null
            , IFileSystem fs = null
            , IRegistry registry = null
            , IProcessServices ps = null) {
            ServiceManager
                .AddService(UIThreadHelper.Instance)
                .AddService(log ?? Substitute.For<IActionLog>())
                .AddService(new SecurityServiceStub())
                .AddService(loggingPermissions ?? Substitute.For<ILoggingPermissions>())
                .AddService(fs ?? new FileSystem())
                .AddService(registry ?? new RegistryImpl())
                .AddService(ps ?? new ProcessServices())
                .AddService(new TestTaskService())
                .AddService(new TestUIServices())
                .AddService(new TestImageService())
                .AddService(new TestPlatformServices())
                .AddService(new RInstallation());
        }

        public string ApplicationName => "RTVS_Test";
        public int LocaleId => 1033;

        public IServiceContainer Services => ServiceManager;

#pragma warning disable 67
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 67
        public bool IsUnitTestEnvironment => true;

        #region IMainThread
        public int ThreadId => UIThreadHelper.Instance.Thread.ManagedThreadId;

        public void Post(Action action, CancellationToken cancellationToken) =>
            UIThreadHelper.Instance.InvokeAsync(action, cancellationToken).DoNotWait();
        #endregion

        #region IIdleTimeSource
        public void DoIdle() {
            UIThreadHelper.Instance.Invoke(() => Idle?.Invoke(null, EventArgs.Empty));
            UIThreadHelper.Instance.DoEvents();
        }
        #endregion
    }
}