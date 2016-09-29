// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {
    [Export(typeof(ITelemetryService))]
    internal sealed class VsTelemetryService : TelemetryServiceBase, ITelemetryLog {
        public static readonly string EventNamePrefixString = "VS/RTools/";
        public static readonly string PropertyNamePrefixString = "VS.RTools.";

        public VsTelemetryService()
            //: base(VsTelemetryService.EventNamePrefixString, VsTelemetryService.PropertyNamePrefixString, new StringTelemetryRecorder()) {
            : base(VsTelemetryService.EventNamePrefixString, VsTelemetryService.PropertyNamePrefixString, VsTelemetryRecorder.Current) {
            }

        #region ITelemetryLog
        public string SessionLog {
            get { return (base.TelemetryRecorder as ITelemetryLog)?.SessionLog; }
        }

        public void Reset() {
            (base.TelemetryRecorder as ITelemetryLog)?.Reset();
        }
        #endregion
    }
}
