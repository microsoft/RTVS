// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Utility {
    public static class StandardServicesMock {
        public static IStandardServices Create() {
            return new StandardServices(
                Substitute.For<ICoreShell>(),
                Substitute.For<ITelemetryService>(),
                Substitute.For<IActionLog>(),
                Substitute.For<IFileSystem>(),
                Substitute.For<IRegistry>(),
                Substitute.For<IProcessServices>());
        }
    }
}
