// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client {
    public static class UriExtensions {
        /// <summary>
        /// Checks machine status via ping.
        /// </summary>
        /// <returns>Empty string if machine is online, exception message if ping failed.</returns>
        public static async Task<string> GetMachineOnlineStatusAsync(this Uri url, int timeout = 3000) {
            if (url.IsFile) {
                return string.Empty;
            }

            try {
                using (TcpClient pingClient = new TcpClient()) {
                    var pingTask = pingClient.ConnectAsync(url.Host, url.Port);
                    if (await Task.WhenAny(pingTask, Task.Delay(timeout)) == pingTask) {
                        await pingTask;
                    } else {
                        return Resources.Error_PingTimedOut.FormatInvariant(url.Host, url.Port);
                    }
                    return string.Empty;
                }
            } catch (SocketException sx) {
                return sx.Message;
            }
        }
    }
}
