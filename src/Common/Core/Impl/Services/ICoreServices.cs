// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.Common.Core.Services {
    public interface ICoreServices: IServiceBag {
        ICoreShell CoreShell { get; }
        IActionLog Log { get; }
        ITelemetryService TelemetryService { get; }
        IFileSystem FileSystem { get; }
        IProcessServices ProcessServices { get; }
        IRegistry Registry { get; }
    }
}
