// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.R.Host.Client {
    internal static class NativeMethods {
        public const int MAX_PATH = 260;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("kernel32.dll")]
        public static extern int GetSystemDefaultLCID();

        [DllImport("kernel32.dll")]
        public static extern int GetOEMCP();

        [DllImport("credui", CharSet = CharSet.Auto)]
        public static extern int CredUIConfirmCredentials(string pszTargetName, bool bConfirm);
    }
}
