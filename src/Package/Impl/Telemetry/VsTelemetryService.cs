// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {
    internal sealed class VsTelemetryService : TelemetryServiceBase<VsTelemetryRecorder> {
        public static readonly string EventNamePrefixString = "VS/RTools/";
        public static readonly string PropertyNamePrefixString = "VS.RTools.";

        public VsTelemetryService() : base(EventNamePrefixString, PropertyNamePrefixString, (VsTelemetryRecorder)VsTelemetryRecorder.Current) {}
    }
}
