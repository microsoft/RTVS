// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Common.Core {
    public static class SecureStringExtensions {
        public static SecureString ToSecureString(this string s) {
            if (s == null) {
                return null;
            }
            var sec = new SecureString();
            foreach (var ch in s) {
                sec.AppendChar(ch);
            }
            return sec;
        }

        public static string ToUnsecureString(this SecureString ss) {
            if (ss == null) {
                return null;
            }

            var ptr = IntPtr.Zero;
            try {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(ss);
                return Marshal.PtrToStringUni(ptr);
            } finally {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}
