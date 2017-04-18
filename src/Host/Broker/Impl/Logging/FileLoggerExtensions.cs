// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.Broker.Logging {
    public static class FileLoggerExtensions {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, string name, string logFolder) {
            factory.AddProvider(new FileLoggerProvider(name, logFolder));
            return factory;
        }
    }
}