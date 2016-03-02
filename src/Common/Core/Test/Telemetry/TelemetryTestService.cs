// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Telemetry;

namespace Microsoft.Common.Core.Test.Telemetry {
    [ExcludeFromCodeCoverage]
    public sealed class TelemetryTestService : TelemetryServiceBase, ITelemetryTestSupport {
        public static readonly string EventNamePrefixString = "Test/RTVS/";
        public static readonly string PropertyNamePrefixString = "Test.RTVS.";

        public TelemetryTestService(string eventNamePrefix, string propertyNamePrefix) :
            base(eventNamePrefix, propertyNamePrefix, new TestTelemetryRecorder()) {

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
        #endregion
    }
}
