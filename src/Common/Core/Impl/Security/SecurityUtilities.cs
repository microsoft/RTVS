// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.Security {
    public static class SecurityUtilities {
        public static IntPtr CreatePasswordBuffer() {
            return Marshal.AllocCoTaskMem(CREDUI_MAX_PASSWORD_LENGTH);
        }

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

        public static IntPtr CreateSecureStringBuffer(int length) {
            var sec = new SecureString();
            for (int i = 0; i <= length; i++) {
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

        public static string ToUnsecureString(this SecureString ss) {
            if (ss == null) {
                return null;
            }

            IntPtr ptr = IntPtr.Zero;
            try {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(ss);
                return Marshal.PtrToStringUni(ptr);
            } finally {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        public static bool DeleteCredentials(string authority) {
            return CredDelete(authority, CRED_TYPE.GENERIC, 0);
        }

        public static Credentials ReadCredentials(string authority) {
            using (CredentialHandle ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    CredentialData credData = ch.GetCredentialData();
                    return Credentials.CreateSavedCredentails(credData.UserName, SecureStringFromNativeBuffer(credData.CredentialBlob));
                }
                return null;
            }
        }

        public static void WriteCredentials(string authority, Credentials credentials) {
            if(!credentials.IsSaved()) {
                CredentialData creds = default(CredentialData);
                try {
                    creds.TargetName = authority;
                    // We have to save the credentials even if user selected NOT to save. Otherwise, user will be asked to enter
                    // credentials for every REPL/intellisense/package/Connection test request. This can provide the best user experience.
                    // We can limit how long the information is saved, in the case whee user selected not to save the credential persistence
                    // is limited to the current log on session. The credentials will not be available if the use logs off and back on.
                    creds.Persist = credentials.CanSave() ? CRED_PERSIST.CRED_PERSIST_ENTERPRISE : CRED_PERSIST.CRED_PERSIST_SESSION;
                    creds.Type = CRED_TYPE.GENERIC;
                    creds.UserName = credentials.UserName;
                    creds.CredentialBlob = Marshal.SecureStringToCoTaskMemUnicode(credentials.Password);
                    creds.CredentialBlobSize = (uint)(credentials.Password.Length * sizeof(ushort));
                    if (!CredWrite(ref creds, 0)) {
                        var error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error, Resources.Error_CredWriteFailed);
                    }
                } finally {
                    if (creds.CredentialBlob != IntPtr.Zero) {
                        Marshal.ZeroFreeCoTaskMemUnicode(creds.CredentialBlob);
                    }
                }
            }
        }
    }
}
