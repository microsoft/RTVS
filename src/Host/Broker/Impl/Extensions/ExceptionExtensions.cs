// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker {
    public static class ExceptionExtensions {
        public static void HandleWebHostStartExceptions(this Exception ex, IServiceProvider services, bool isService) {
            var configuration = services.GetRequiredService<IConfigurationRoot>();
            var lifetime = services.GetRequiredService<IApplicationLifetime>();
            var logger = services.GetService<ILogger<StartupBase>>();

            switch (ex) {
                case IOException ioException when (ioException.InnerException as AggregateException).IsPortInUseException():
                case AggregateException aggregateException when aggregateException.IsPortInUseException():
                    var uri = GetServerUri(configuration);
                    logger?.LogError(0, ex, Resources.Error_ConfiguredPortNotAvailable, uri?.Port);
                    lifetime.StopApplication();
                    if (!isService) {
                        Environment.Exit((int)BrokerExitCodes.PortInUse);
                    }
                    return;
                default:
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return;
            }
        }
        
        private static Uri GetServerUri(IConfiguration configuration) {
            var url = configuration.GetValue<string>(WebHostDefaults.ServerUrlsKey, null);
            if (url != null && Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.Port != 0) {
                return uri;
            }
            return null;
        }

        private static bool IsPortInUseException(this AggregateException aggex) => 
            aggex != null && aggex.InnerExceptions.Count == 1 && (aggex.InnerException as UvException)?.StatusCode == -4091;
    }
}
