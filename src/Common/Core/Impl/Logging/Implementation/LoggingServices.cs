// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Logging.Implementation {
    internal sealed class LoggingServices : ILoggingServices {
        private readonly IApplicationConstants _appConstants;
        private Logger _instance;

        public LoggingServices(ILoggingPermissions permissions, IApplicationConstants appConstants) {
            Permissions = permissions;
            _appConstants = appConstants;
        }

        public ILoggingPermissions Permissions { get; }

        public IActionLog GetOrCreateLog(string appName) {
            if (_instance == null) {
                var instance = new Logger(_appConstants.ApplicationName, Path.GetTempPath(), Permissions);
                Interlocked.CompareExchange(ref _instance, instance, null);
            }
            return _instance;
        }

        public void Dispose() {
            _instance?.Dispose();
        }
    }
}
