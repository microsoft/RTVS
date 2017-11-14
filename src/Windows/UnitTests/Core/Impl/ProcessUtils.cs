// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using static Microsoft.UnitTests.Core.Windows.NativeMethods;

namespace Microsoft.UnitTests.Core {
    public static class ProcessUtils {
        public static List<int> GetPortByProcessId(int pid) {
            List<int> ports = new List<int>();
            int nBufSize = 0;
            int err = GetTcpTable2(IntPtr.Zero, ref nBufSize, false);
            if (err == 0x7a) { // ERROR_INSUFFICIENT_BUFFER
                IntPtr pBuf = Marshal.AllocHGlobal(nBufSize);
                try {
                    err = GetTcpTable2(pBuf, ref nBufSize, false);
                    if (err == 0) {
                        int entrySize = Marshal.SizeOf(typeof(MIB_TCPROW2));
                        int nEntries = Marshal.ReadInt32(pBuf);
                        int tableStartAddr = (int)pBuf + sizeof(int);
                        for (int i = 0; i < nEntries; i++) {
                            IntPtr pEntry = (IntPtr)(tableStartAddr + i * entrySize);
                            MIB_TCPROW2 tcpData = (MIB_TCPROW2)Marshal.PtrToStructure(pEntry, typeof(MIB_TCPROW2));
                            if (tcpData.dwOwningPid == pid) {
                                ports.Add(ntohs((ushort)tcpData.dwLocalPort));
                            }
                        }
                    } else {
                        throw new Win32Exception(err);
                    }
                } finally {
                    if (pBuf != IntPtr.Zero) {
                        Marshal.FreeHGlobal(pBuf);
                    }
                }
            } else {
                throw new Win32Exception(err);
            }
            return ports.Distinct().ToList();
        }
    }
}
