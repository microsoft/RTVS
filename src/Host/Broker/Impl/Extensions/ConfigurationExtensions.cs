// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Start;

namespace Microsoft.R.Host.Broker {
    public static class ConfigurationExtensions {
        public static LoggingOptions GetLoggingOptions(this IConfiguration configuration) => configuration
            .GetLoggingSection()
            .Get<LoggingOptions>();

        public static StartupOptions GetStartupOptions(this IConfiguration configuration) => configuration
            .GetStartupSection()
            .Get<StartupOptions>();

        public static IConfigurationSection GetLifetimeSection(this IConfiguration configuration) => configuration.GetSection("lifetime");
        public static IConfigurationSection GetLoggingSection(this IConfiguration configuration) => configuration.GetSection("logging");
        public static IConfigurationSection GetRSection(this IConfiguration configuration) => configuration.GetSection("r");
        public static IConfigurationSection GetSecuritySection(this IConfiguration configuration) => configuration.GetSection("security");
        public static IConfigurationSection GetStartupSection(this IConfiguration configuration) => configuration.GetSection("startup");
    }
}