// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit.Sdk;

namespace Microsoft.Common.Core.Test.Fixtures {
    public class CoreServicesFixture : MethodFixtureBase, ICoreServices {
        private readonly ICoreServices _services;
        private readonly LogProxy _log;

        public IActionLog Log => _log;
        public IFileSystem FileSystem => _services.FileSystem;
        public ILoggingPermissions LoggingPermissions => _services.LoggingPermissions;
        public IProcessServices ProcessServices => _services.ProcessServices;
        public IRegistry Registry => _services.Registry;
        public ISecurityService Security => _services.Security;
        public ITelemetryService Telemetry => _services.Telemetry;
        public ITaskService Tasks => _services.Tasks;
        public IMainThread MainThread => _services.MainThread;

        public CoreServicesFixture() {
            _services = TestCoreServices.CreateReal();
            _log = new LogProxy();
            _log.SetLog(_services.Log);
        }

        public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            try {
                var logsFolder = Path.Combine(DeployFilesFixture.TestFilesRoot, "Logs");
                Directory.CreateDirectory(logsFolder);
                _log.SetLog(new Logger(testInput.FileSytemSafeName, logsFolder, _services.LoggingPermissions));
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

        public class LogProxy : IActionLog {
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
