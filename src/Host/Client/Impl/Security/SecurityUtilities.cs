// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.R.Host.Client.Security {
    public static class SecurityUtilities {
        public static IntPtr CreatePasswordBuffer() {
            return CreateSecureStringBuffer(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH);
        }

        public static IntPtr CreateSecureStringBuffer(int length) {
            var initial = new char[length + 1];
            var sec = new SecureString();
            for (int i = 0; i < length + 1; i++) {
                sec.AppendChar('\0');
            }
            return Marshal.SecureStringToGlobalAllocUnicode(sec);
        }

        public static SecureString SecureStringFromNativeBuffer(IntPtr nativeBuffer) {
            var ss = new SecureString();
            unsafe
            {
                for (char* p = (char*)nativeBuffer; *p != '\0'; p++) {
                    ss.AppendChar(*p);
                }
            }
            return ss;
        }
    }
}
