using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {

    [Export(typeof(ITelemetryService))]
    internal sealed class VsTelemetryService : TelemetryServiceBase {
        public static readonly string EventNamePrefixString = "VS/RTools/";
        public static readonly string PropertyNamePrefixString = "VS.RTools.";

        private static Lazy<VsTelemetryService> _instance = Lazy.Create(() => new VsTelemetryService());

        public VsTelemetryService()
            : base(VsTelemetryService.EventNamePrefixString, VsTelemetryService.PropertyNamePrefixString, VsTelemetryRecorder.Current) {
        }

        public static TelemetryServiceBase Current {
            get {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Start a telemetry activity, dispose of the return value when the activity is complete
        /// </summary>
        public override ITelemetryActivity StartActivity(TelemetryArea area, string eventName) {
            Check.ArgumentStringNullOrEmpty("eventName", eventName);

            string fullEventName = this.EventNamePrefix + area.ToString() + "/" + eventName;
            string eventPropertyPrefix = this.PropertyNamePrefix + area.ToString() + "." + eventName + '.';

            return new TelemetryActivityWrapper(this.TelemetryRecorder, fullEventName, eventPropertyPrefix);
        }
    }
}
