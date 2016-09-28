// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading;

namespace Microsoft.Common.Core.Logging.Implementation {
    [Export(typeof(ILoggingServices))]
    internal sealed class LoggingServices : ILoggingServices {
        private static Logger _instance;

        [ImportingConstructor]
        public LoggingServices(ILoggingPermissions permissions) {
            Permissions = permissions;
        }

        public ILoggingPermissions Permissions { get; }

        public IActionLog OpenLog(string appName) {
            if (_instance == null) {
                var instance = new Logger(appName, Permissions, writer: null);
                Interlocked.Exchange(ref _instance, instance);
            }
            return _instance;
        }

        public void Dispose() {
            _instance?.Dispose();
        }
    }
}
