using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {

    internal sealed class VsTelemetryService : TelemetryServiceBase, ITelemetryLog {
        public static readonly string EventNamePrefixString = "VS/RTools/";
        public static readonly string PropertyNamePrefixString = "VS.RTools.";

        private static Lazy<VsTelemetryService> _instance = Lazy.Create(() => new VsTelemetryService());

        public VsTelemetryService(ITelemetryRecorder recorder = null)
            : base(VsTelemetryService.EventNamePrefixString, VsTelemetryService.PropertyNamePrefixString, new StringTelemetryRecorder()) {
//            : base(VsTelemetryService.EventNamePrefixString, VsTelemetryService.PropertyNamePrefixString, VsTelemetryRecorder.Current) {
        }

        public static TelemetryServiceBase Current => _instance.Value;

        #region ITelemetryLog
        public string SessionLog {
            get { return (base.TelemetryRecorder as ITelemetryLog)?.SessionLog; }
        }

        public void Reset() {
            (base.TelemetryRecorder as ITelemetryLog)?.Reset();
        }
        #endregion

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
