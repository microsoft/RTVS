// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Logging;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class LoggingPermissions: ILoggingPermissions {
        public LoggingPermissions() {
            CurrentVerbosity = LogVerbosity.Traffic;
        }

        public LogVerbosity MaxVerbosity => LogVerbosity.Traffic;
        public bool IsFeedbackPermitted => true;
        public LogVerbosity CurrentVerbosity { get; set; }
    }
}
