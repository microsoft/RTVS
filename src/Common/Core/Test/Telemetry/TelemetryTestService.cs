using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.Common.Core.Tests.Telemetry {
    [ExcludeFromCodeCoverage]
    public sealed class TelemetryTestService : TelemetryServiceBase, ITelemetryTestSupport {
        public static readonly string EventNamePrefixString = "Test/RTVS/";
        public static readonly string PropertyNamePrefixString = "Test.RTVS.";

        public TelemetryTestService(string eventNamePrefix, string propertyNamePrefix) :
            base(eventNamePrefix, propertyNamePrefix, new StringTelemetryRecorder()) {

        }

        public TelemetryTestService() :
            this(TelemetryTestService.EventNamePrefixString, TelemetryTestService.PropertyNamePrefixString) {
        }

        #region ITelemetryTestSupport
        public string SessionLog {
            get {
                ITelemetryTestSupport testSupport = this.TelemetryRecorder as ITelemetryTestSupport;
                return testSupport.SessionLog;
            }
        }

        public void Reset() {
            ITelemetryTestSupport testSupport = this.TelemetryRecorder as ITelemetryTestSupport;
            testSupport.Reset();
        }

        /// <summary>
        /// Start a telemetry activity, dispose of the return value when the activity is complete
        /// </summary>
        public override ITelemetryActivity StartActivity(TelemetryArea area, string eventName) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
