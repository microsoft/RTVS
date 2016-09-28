// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.Common.Core.Services {
    [Export(typeof(ICoreServices))]
    public sealed class CoreServices : ICoreServices {
        private readonly ServiceBag _services = new ServiceBag();
        private IActionLog _log;

        [ImportingConstructor]
        public CoreServices(
              ICoreShell coreShell
            , ITelemetryService telemetry
            , [Import(AllowDefault = true)] IActionLog log
            , [Import(AllowDefault = true)] IFileSystem fs = null
            , [Import(AllowDefault = true)] IRegistry registry = null
            , [Import(AllowDefault = true)] IProcessServices ps = null) {

            _log = log;
            CoreShell = coreShell;
            TelemetryService = telemetry;
            ProcessServices = ps ?? new ProcessServices();
            Registry = registry ?? new RegistryImpl();
            FileSystem = fs ?? new FileSystem();

            _services.Add(CoreShell)
                     .Add(Log)
                     .Add(TelemetryService)
                     .Add(ProcessServices)
                     .Add(Registry)
                     .Add(FileSystem);
        }

        public IActionLog Log {
            get {
                if (_log == null) {
                    var ls = CoreShell.ExportProvider.GetExportedValueOrDefault<ILoggingServices>();
                    _log = ls.OpenLog(null);
                }
                return _log;
            }
        }

        public ICoreShell CoreShell { get; }
        public IFileSystem FileSystem { get; } 
        public IProcessServices ProcessServices { get; }
        public IRegistry Registry { get; } 
        public ITelemetryService TelemetryService { get; }

        public object GetService(Type serviceType) {
            return _services.GetService(serviceType);
        }

        public T GetService<T>() where T : class {
            return _services.GetService<T>();
        }
    }
}
