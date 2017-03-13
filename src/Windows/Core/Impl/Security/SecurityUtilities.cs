// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Common.Core.Security {
    public static class SecurityUtilities {
        public static IntPtr CreateSecureStringBuffer(int length) {
            var sec = new SecureString();
            for (int i = 0; i <= length; i++) {
                sec.AppendChar('\0');
            }
            return Marshal.SecureStringToGlobalAllocUnicode(sec);
        }

        public static SecureString SecureStringFromNativeBuffer(IntPtr nativeBuffer) {
            var ss = new SecureString();
            unsafe {
                for (char* p = (char*)nativeBuffer; *p != '\0'; p++) {
                    ss.AppendChar(*p);
                }
            }
            return ss;
        }
    }
}
