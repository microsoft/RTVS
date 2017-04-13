// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Logging;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.Common.Core.Test.Telemetry;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit.Sdk;

namespace Microsoft.Common.Core.Test.Fixtures {
    public class ServiceManagerFixture : MethodFixtureBase, IServiceManager {
        private readonly LogProxy _log;
        private readonly IServiceManager _serviceManager;

        public ServiceManagerFixture() {
            _log = new LogProxy();
            _serviceManager = new ServiceManager();
            _serviceManager
                .AddService(UIThreadHelper.Instance)
                .AddService(_log)
                .AddService(new SecurityServiceStub())
                .AddService(new MaxLoggingPermissions())
                .AddService(new TelemetryTestService())
                .AddService(new FileSystem())
                .AddService(new RegistryImpl())
                .AddService(new ProcessServices())
                .AddService(new TestUIServices())
                .AddService(new TestPlatformServices());
        }

        public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            try {
                var logsFolder = Path.Combine(DeployFilesFixture.TestFilesRoot, "Logs");
                Directory.CreateDirectory(logsFolder);
                _log.SetLog(new Logger(testInput.FileSytemSafeName, logsFolder, new MaxLoggingPermissions()));
            } catch (Exception) {
                return Task.FromResult(Task.FromResult(new RunSummary {Failed = 1}));
            }

            return base.InitializeAsync(testInput, messageBus);
        }

        public override Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
            if (result.Failed > 0) {
                _log.Flush();
            }
            return base.DisposeAsync(result, messageBus);
        }

        #region IServiceContainer
        public T GetService<T>(Type type = null) where T : class => _serviceManager.GetService<T>(type);
        public IEnumerable<Type> AllServices => _serviceManager.AllServices;
        public IEnumerable<T> GetServices<T>() where T : class => _serviceManager.GetServices<T>();

#pragma warning disable 67
        public event EventHandler<ServiceContainerEventArgs> ServiceAdded;
        public event EventHandler<ServiceContainerEventArgs> ServiceRemoved;
#pragma warning restore 67
        #endregion

        #region IServiceManager
        public void Dispose() { }
        public IServiceManager AddService<T>(T service, Type type = null) where T : class => _serviceManager.AddService(service, type);
        public IServiceManager AddService<T>(Func<T> factory) where T : class => _serviceManager.AddService(factory);
        public IServiceManager AddService(Type type) => _serviceManager.AddService(type);
        public void RemoveService(object service) => _serviceManager.RemoveService(service);
        #endregion

        private class LogProxy : IActionLog {
            private IActionLog _log;

            public void SetLog(IActionLog log) {
                _log = log;
            }

            public void Write(LogVerbosity verbosity, MessageCategory category, string message) 
                => _log.Write(verbosity, category, message);

            public void WriteFormat(LogVerbosity verbosity, MessageCategory category, string format, params object[] arguments)
                => _log.WriteFormat(verbosity, category, format, arguments);

            public void WriteLine(LogVerbosity verbosity, MessageCategory category, string message)
                => _log.WriteLine(verbosity, category, message);

            public void Flush() => _log.Flush();

            public LogVerbosity LogVerbosity => _log.LogVerbosity;
            public string Folder => _log.Folder;
        }
    }
}
