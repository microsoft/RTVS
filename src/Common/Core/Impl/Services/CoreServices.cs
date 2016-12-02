// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Logging.Implementation;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Core.Services {
    public sealed class CoreServices : ICoreServices {
        public CoreServices(IApplicationConstants appConstants
            , ITelemetryService telemetry
            , ISettingsStorage settings
            , ITaskService tasks
            , IMainThread mainThread
            , ICoreShell coreShell) {
            Telemetry = telemetry;
            Registry = new RegistryImpl();
            Security = new SecurityService(coreShell);
            LoggingServices = new LoggingServices(new LoggingPermissions(appConstants, telemetry, Registry), appConstants);
            Tasks = tasks;

            ProcessServices = new ProcessServices();
            FileSystem = new FileSystem();
            Settings = settings;
            MainThread = mainThread;

            Log = LoggingServices.GetOrCreateLog(appConstants.ApplicationName);
        }

        public CoreServices(IApplicationConstants appConstants
            , ITelemetryService telemetry
            , ILoggingPermissions permissions
            , ISecurityService security
            , ITaskService tasks
            , ISettingsStorage settings
            , IMainThread mainThread
            , IActionLog log
            , IFileSystem fs
            , IRegistry registry
            , IProcessServices ps) {

            LoggingServices = new LoggingServices(permissions, appConstants);
            Log = log;

            Telemetry = telemetry;
            Security = security;
            Tasks = tasks;

            ProcessServices = ps;
            Registry = registry;
            FileSystem = fs;
            Settings = settings;
            MainThread = mainThread;
        }

        public IActionLog Log { get; }
        public IFileSystem FileSystem { get; } 
        public IProcessServices ProcessServices { get; }
        public IRegistry Registry { get; } 
        public ISecurityService Security { get; }
        public ITelemetryService Telemetry { get; }
        public ITaskService Tasks { get; }
        public ILoggingServices LoggingServices { get; }
        public ISettingsStorage Settings { get; }
        public IMainThread MainThread { get; }
    }
}
