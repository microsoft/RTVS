// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        private readonly FileLogWriterProxy _fileLogWriterProxy;
        private readonly ICoreServices _services;

        public IActionLog Log => _services.Log;
        public IFileSystem FileSystem => _services.FileSystem;
        public ILoggingServices LoggingServices => _services.LoggingServices;
        public IProcessServices ProcessServices => _services.ProcessServices;
        public IRegistry Registry => _services.Registry;
        public ISecurityService Security => _services.Security;
        public ITelemetryService Telemetry => _services.Telemetry;
        public ITaskService Tasks => _services.Tasks;
        public IMainThread MainThread => _services.MainThread;

        public CoreServicesFixture() {
            _fileLogWriterProxy = new FileLogWriterProxy();
            _services = TestCoreServices.CreateReal(_fileLogWriterProxy);
        }

        public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            try {
                _fileLogWriterProxy.SetLogWriter(FileLogWriter.InFolder(DeployFilesFixture.TestFilesRoot, testInput.FileSytemSafeName, int.MaxValue, 0));
            } catch (Exception) {
                return Task.FromResult(Task.FromResult(new RunSummary {Failed = 1}));
            }

            return base.InitializeAsync(testInput, messageBus);
        }

        public override Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
            if (result.Failed > 0) {
                _fileLogWriterProxy.Flush();
            }
            return base.DisposeAsync(result, messageBus);
        }

        private class FileLogWriterProxy : IActionLogWriter {
            private IActionLogWriter _logWriter;

            public void SetLogWriter(IActionLogWriter logWriter) {
                _logWriter = logWriter;
            }

            public void Write(MessageCategory category, string message) {
                _logWriter?.Write(category, message);
            }

            public void Flush() {
                _logWriter?.Flush();
            }
        }
    }
}
