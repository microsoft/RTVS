// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Common.Core.Net {
    public static class PortUtil {
        private static Random _rand = new Random();
        public static int GetAvailablePort(int minPort, int maxPort) {
            int port = _rand.Next(minPort, maxPort + 1);
            while (!IsPortAvailable(port)) {
                port = _rand.Next(minPort, maxPort + 1);
            }
            return port;
        }

        public static bool IsPortAvailable(int port) {
            try {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();

                // port is available
                listener.Stop();
                return true;
            } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) {
                return false;
            }
        }
    }
}
