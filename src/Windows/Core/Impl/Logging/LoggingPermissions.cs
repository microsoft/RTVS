// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
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
    public sealed class LoggingPermissions : ILoggingPermissions {
        internal const string LogVerbosityValueName = "LogVerbosity";
        internal const string FeedbackValueName = "Feedback";

        private readonly ITelemetryService _telemetryService;
        private readonly IRegistry _registry;

        private LogVerbosity _currentVerbosity;
        private LogVerbosity? _registryVerbosity;
        private int? _registryFeedbackSetting;

        public LoggingPermissions(IServiceContainer services) {
            _telemetryService = services.Telemetry();
            _registry = services.GetService<IRegistry>();

            _registryVerbosity = GetLogLevelFromRegistry();
            _registryFeedbackSetting = GetFeedbackFromRegistry();

            // Default value for CurrentVerbosity should match default value in IRSettings. 
            // https://github.com/Microsoft/RTVS/issues/2705
            CurrentVerbosity = LogVerbosity.Normal;
        }

        public LogVerbosity CurrentVerbosity {
            get => _currentVerbosity;
            set => _currentVerbosity = MathExtensions.Min(value, MaxVerbosity);
        }

        public LogVerbosity MaxVerbosity => GetEffectiveVerbosity();

        public bool IsFeedbackPermitted => GetEffectiveFeedbackSetting();

        private LogVerbosity GetEffectiveVerbosity() {
            LogVerbosity adminSetValue;
            if (_telemetryService.IsEnabled) {
                adminSetValue = _registryVerbosity ?? LogVerbosity.Traffic;
                return MathExtensions.Min(adminSetValue, LogVerbosity.Traffic);
            }
            // If telemetry is disabled, registry setting allows increase in the logging level.
            adminSetValue = _registryVerbosity ?? LogVerbosity.None;
            return MathExtensions.Max(adminSetValue, LogVerbosity.Minimal);
        }

        private bool GetEffectiveFeedbackSetting() {
            int adminSetValue;
            if (_telemetryService.IsEnabled) {
                adminSetValue = _registryFeedbackSetting ?? 1;
                return MathExtensions.Min(adminSetValue, 1) == 1;
            }
            // If telemetry is disabled, registry setting allows enabling the feedback.
            adminSetValue = _registryFeedbackSetting ?? 0;
            return MathExtensions.Max(adminSetValue, 0) == 1;
        }

        private LogVerbosity? GetLogLevelFromRegistry() 
            => (LogVerbosity?)GetValueFromRegistry(LogVerbosityValueName, (int)LogVerbosity.None, (int)LogVerbosity.Traffic);

        private int? GetFeedbackFromRegistry() => GetValueFromRegistry(FeedbackValueName, 0, 1);

        private int? GetValueFromRegistry(string name, int minValue, int maxValue) {
            if(_registry.LocalMachineHive == null) {
                return maxValue;
            }
            using (var hlkm = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                try {
                    using (var key = hlkm.OpenSubKey(_registry.LocalMachineHive)) {
                        var value = (int?)key.GetValue(name);
                        if (value.HasValue && value.Value >= minValue && value.Value <= maxValue) {
                            return value;
                        }
                    }
                } catch (Exception) { }
            }
            return null;
        }
    }
}
