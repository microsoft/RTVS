// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Logging.Implementation;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.Common.Core.Services {
    [Export(typeof(ICoreServices))]
    public sealed class CoreServices : ICoreServices {
        private readonly IApplicationConstants _appConstants;
        private IActionLog _log;

        [ImportingConstructor]
        public CoreServices(
              IApplicationConstants appConstants
            , ITelemetryService telemetry
            , ILoggingPermissions permissions
            , ISecurityService security
            , IColorService colorService
            , [Import(AllowDefault = true)] IActionLog log = null
            , [Import(AllowDefault = true)] IFileSystem fs = null
            , [Import(AllowDefault = true)] IRegistry registry = null
            , [Import(AllowDefault = true)] IProcessServices ps = null) {

            LoggingServices = new LoggingServices(permissions, appConstants);
            _appConstants = appConstants;
            _log = log;

            Telemetry = telemetry;
            Security = security;
            ColorService = colorService;

            ProcessServices = ps ?? new ProcessServices();
            Registry = registry ?? new RegistryImpl();
            FileSystem = fs ?? new FileSystem();
        }

        public IActionLog Log {
            get {
                if (_log == null) {
                    _log = LoggingServices.GetOrCreateLog(_appConstants.ApplicationName);
                }
                return _log;
            }
        }

        public IFileSystem FileSystem { get; } 
        public IProcessServices ProcessServices { get; }
        public IRegistry Registry { get; } 
        public ISecurityService Security { get; }
        public ITelemetryService Telemetry { get; }
        public ILoggingServices LoggingServices { get; }
        public IColorService ColorService { get; }
    }
}
