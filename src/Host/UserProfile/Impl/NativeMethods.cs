// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.R.Host.UserProfile {
    internal static class NativeMethods {
        public const int MAX_PATH = 260;

        [DllImport("userenv.dll", CharSet = CharSet.Auto)]
        public static extern uint CreateProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
            [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszProfilePath,
            uint cchProfilePath);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]

        public static extern bool DeleteProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string lpSidString,
            [MarshalAs(UnmanagedType.LPWStr)] string lpProfilePath,
            [MarshalAs(UnmanagedType.LPWStr)] string lpComputerName);

    }
}
