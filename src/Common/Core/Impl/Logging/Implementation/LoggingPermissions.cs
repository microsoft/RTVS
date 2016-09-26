// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Win32;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Determines logging permissions depending on the current 
    /// telemetry settings and registry keys.
    /// </summary>
    /// <remarks>
    /// Rules:
    ///     a. Feedback and logging is permitted if telemetry is enabled. If telemetry is off, 
    ///        admin can selectively enable feedback and logging in registry under HKLM.
    ///     b. If telemetry is on, default logging level is 'Normal'. This allows recording of all events
    ///        but security and privacy related data such as cross-process traffic or commands and data 
    ///        in the interactive window.
    ///     c. Admin can limit level of logging and disable feedback sending even if app telemetry is on.
    /// </remarks>
    [Export(typeof(ILoggingPermissions))]
    internal sealed class LoggingPermissions : ILoggingPermissions {
        internal const string LogLevelValueName = "LogLevel";
        internal const string FeedbackValueName = "Feedback";

        private readonly IApplicationConstants _appConstants;
        private readonly ITelemetryService _telemetryService;
        private readonly IRegistry _registry;

        private LogLevel _currentLogLevel;
        private LogLevel? _registryLogLevel;
        private int? _registryFeedbackSetting;

        public LoggingPermissions(IApplicationConstants appConstants, ITelemetryService telemetryService, IRegistry registry) {
            _appConstants = appConstants;
            _telemetryService = telemetryService;
            _registry = registry;

            _registryLogLevel = GetLogLevelFromRegistry();
            _registryFeedbackSetting = GetFeedbackFromRegistry();
        }

        public LogLevel Current {
            get { return _currentLogLevel; }
            set { _currentLogLevel = MathExtensions.Min(value, MaxLogLevel); }
        }

        public LogLevel MaxLogLevel => GetEffectiveLogLevel();

        public bool IsFeedbackPermitted => GetEffectiveFeedbackSetting();

        private LogLevel GetEffectiveLogLevel() {
            LogLevel adminSetValue;
            if (_telemetryService.IsEnabled) {
                adminSetValue = _registryLogLevel.HasValue ? _registryLogLevel.Value : LogLevel.Traffic;
                return MathExtensions.Min(adminSetValue, LogLevel.Traffic);
            }
            // If telemetry is disabled, registry setting allows increase in the logging level.
            adminSetValue = _registryLogLevel.HasValue ? _registryLogLevel.Value : LogLevel.None;
            return MathExtensions.Max(_registryLogLevel.HasValue ? _registryLogLevel.Value : LogLevel.None, LogLevel.Minimal);
        }

        private bool GetEffectiveFeedbackSetting() {
            int adminSetValue;
            if (_telemetryService.IsEnabled) {
                adminSetValue = _registryFeedbackSetting.HasValue ? _registryFeedbackSetting.Value : 1;
                return MathExtensions.Min(adminSetValue, 1) == 1;
            }
            // If telemetry is disabled, registry setting allows enabling the feedback.
            adminSetValue = _registryFeedbackSetting.HasValue ? _registryFeedbackSetting.Value : 0;
            return MathExtensions.Max(adminSetValue, 0) == 1;
        }

        private LogLevel? GetLogLevelFromRegistry() {
            return (LogLevel?)GetValueFromRegistry(LogLevelValueName, (int)LogLevel.None, (int)LogLevel.Traffic);
        }

        private int? GetFeedbackFromRegistry() {
            return GetValueFromRegistry(FeedbackValueName, 0, 1);
        }

        private int? GetValueFromRegistry(string name, int minValue, int maxValue) {
            using (var hlkm = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                try {
                    using (var key = hlkm.OpenSubKey(_appConstants.LocalMachineHive)) {
                        var value = (int)key.GetValue(name);
                        if (value >= minValue && value <= maxValue) {
                            return value;
                        }
                    }
                } catch (Exception) { }
            }
            return null;
        }
    }
}
