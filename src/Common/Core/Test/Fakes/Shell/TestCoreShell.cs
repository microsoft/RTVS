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
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public enum TestCoreShellMode {
        Empty,
        Substitute,
        Basic
    }

    [ExcludeFromCodeCoverage]
    public class TestCoreShell : ICoreShell, IIdleTimeSource {
        private readonly ICompositionCatalog _catalog;
        private readonly Thread _creatorThread;

        public IServiceManager ServiceManager { get; }

        /// <summary>
        /// Creates an empty core shell or with with a set of basic services.
        /// Does not delegate to MEF or host-provided (global) service provider
        /// </summary>
        public TestCoreShell(TestCoreShellMode mode = TestCoreShellMode.Basic) {
            _creatorThread = UIThreadHelper.Instance.Thread;
            ServiceManager = new ServiceManager();
            AddServices(mode);
        }

        /// <summary>
        /// Creates test core shell with basic services and delegation
        /// to the supplied export provider for additional services.
        /// </summary>
        /// <param name="exportProvider"></param>
        public TestCoreShell(IExportProvider exportProvider) {
            _creatorThread = UIThreadHelper.Instance.Thread;
            ServiceManager = new TestServiceManager(exportProvider);
            AddBasicServices();
        }

        /// <summary>
        /// Creates test core shell based on the propulated set of services
        /// </summary>
        public TestCoreShell(IServiceManager services) {
            _creatorThread = UIThreadHelper.Instance.Thread;
            ServiceManager = services;
        }

        public TestCoreShell(ICompositionCatalog catalog): this(TestCoreShellMode.Basic) { 
            ServiceManager
                .AddService(catalog)
                .AddService(catalog.ExportProvider)
                .AddService(catalog.CompositionService);
        }

        private void AddServices(TestCoreShellMode mode) {
            switch (mode) {
                case TestCoreShellMode.Substitute:
                    AddSubstiteServices();
                    break;
                case TestCoreShellMode.Basic:
                    AddBasicServices();
                    break;
            }
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
                .AddService(new TestPlatformServices());
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