// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.UnitTests.Core {
    internal static unsafe class NativeMethods {
        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern ushort ntohs(ushort v);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern int GetTcpTable2(IntPtr tcpTable, ref int size, bool bOrder);

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE2 {
            public int dwNumEntries;
            MIB_TCPROW2[] table;
        }

        public enum TCP_CONNECTION_OFFLOAD_STATE : int {
            TcpConnectionOffloadStateInHost = 0,
            TcpConnectionOffloadStateOffloading = 1,
            TcpConnectionOffloadStateOffloaded = 2,
            TcpConnectionOffloadStateUploading = 3,
            TcpConnectionOffloadStateMax = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW2 {
            public int dwState;
            public int dwLocalAddr;
            public int dwLocalPort;
            public int dwRemoteAddr;
            public int dwRemotePort;
            public int dwOwningPid;
            TCP_CONNECTION_OFFLOAD_STATE dwOffloadState;
        }
    }
}
