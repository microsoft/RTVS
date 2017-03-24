// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestCoreShell : ICoreShell, IIdleTimeSource {
        private readonly ICompositionCatalog _catalog;
        private readonly Thread _creatorThread;

        public IServiceManager ServiceManager { get; }

        public TestCoreShell(ICompositionCatalog catalog, IServiceManager services) {
            _catalog = catalog;
            _creatorThread = UIThreadHelper.Instance.Thread;
            ServiceManager = services;
        }

        /// <summary>
        /// Creates core shell with a set of basic services.
        /// Does not delegate to MEF or host-provided (global) 
        /// service provider
        /// </summary>
        public TestCoreShell() {
            _creatorThread = UIThreadHelper.Instance.Thread;
            ServiceManager = new ServiceManager();
            AddBasicServices();
        }

        public TestCoreShell(IExportProvider exportProvider) {
            _creatorThread = UIThreadHelper.Instance.Thread;
            ServiceManager = new TestServiceManager(exportProvider);
            AddBasicServices();
        }

        public TestCoreShell(ICompositionCatalog catalog
        , IActionLog log = null
        , ILoggingPermissions loggingPermissions = null
        , IFileSystem fs = null
        , IRegistry registry = null
        , IProcessServices ps = null) : this(catalog, new TestServiceManager(catalog.ExportProvider)) {
            AddBasicServices(log, loggingPermissions, fs, registry, ps);
            ServiceManager
                .AddService(catalog)
                .AddService(catalog.ExportProvider)
                .AddService(catalog.CompositionService);
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