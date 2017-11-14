// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Telemetry;

namespace Microsoft.R.Platform.Stubs {
    internal sealed class TelemetryServiceStub: ITelemetryService {
        public bool IsEnabled => true;
        public void ReportEvent(TelemetryArea area, string eventName, object parameters = null) { }
    }
}
