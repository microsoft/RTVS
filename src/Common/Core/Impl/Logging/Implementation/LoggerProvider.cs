// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;

namespace Microsoft.Common.Core.Logging.Implementation {
    [Export(typeof(ILoggingServices))]
    internal sealed class LoggingServices : ILoggingServices {
        private static Logger _instance;
        private readonly ILoggingPermissions _permissions;

        [ImportingConstructor]
        public LoggingServices(ILoggingPermissions permissions) {
            _permissions = permissions;
        }

        public IActionLog Open(string appName) {
            if (_instance == null) {
                _instance = new Logger(appName, _permissions.MaxLogLevel, writer: null);
            }
            return _instance;
        }

        public void Dispose() {
            _instance?.Dispose();
        }
    }
}
