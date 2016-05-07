// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.R.Host.Client {
    internal static class NativeMethods {
        public const int MAX_PATH = 260;
        private const int MAX_DEFAULTCHAR = 2;
        private const int MAX_LEADBYTES = 12;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("kernel32.dll")]
        public static extern uint GetUserDefaultLCID();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCPInfoEx(uint codePage, uint dwFlags, out CPINFOEX lpCPInfoEx);

        [StructLayout(LayoutKind.Sequential)]
        public struct CPINFOEX {
            [MarshalAs(UnmanagedType.U4)]
            public int MaxCharSize;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEFAULTCHAR)]
            public byte[] DefaultChar;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_LEADBYTES)]
            public byte[] LeadBytes;

            public char UnicodeDefaultChar;

            [MarshalAs(UnmanagedType.U4)]
            public int CodePage;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PATH)]
            public byte[] CodePageName;
        }
    }
}
