// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Monitor {
    public enum CredType : int {
        GENERIC = 1,
        DOMAIN_PASSWORD = 2,
        DOMAIN_CERTIFICATE = 3,
        DOMAIN_VISIBLE_PASSWORD = 4,
        MAXIMUM = 5
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Credential {
        public int flags;
        public int type;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string targetName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME lastWritten;
        public int credentialBlobSize;
        public IntPtr credentialBlob;
        public int persist;
        public int attributeCount;
        public IntPtr credAttribute;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string targetAlias;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string userName;
    }

    public class CredentialManager {
        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredRead(string target, CredType type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        static extern bool CredWrite([In] ref Credential userCredential, [In] uint flags);


        private const string BrokerUserCredName = "RHostBrokerUserCred";

        public static Task<bool> IsBrokerUserCredentialSavedAsync() {
            return Task.Run(() => {
                Credential cred;
                IntPtr credPtr;

                bool credStatus = false;

                if (CredRead(BrokerUserCredName, CredType.GENERIC, 0, out credPtr)) {
                    cred = (Credential)Marshal.PtrToStructure(credPtr, typeof(Credential));
                    byte[] passwordBytes = new byte[cred.credentialBlobSize];
                    Marshal.Copy(cred.credentialBlob, passwordBytes, 0, cred.credentialBlobSize);

                    // Convert to text
                    string passwordText = Encoding.Unicode.GetString(passwordBytes);
                } else {
                    int error = Marshal.GetLastWin32Error();
                    // ERROR_NOT_FOUND : Credentials were not found
                    if (error == 1168L) {
                        credStatus = false;
                        // TODO: show status message
                    }
                }

                if (!credPtr.Equals(IntPtr.Zero)) {
                    Marshal.DestroyStructure(credPtr, typeof(Credential));
                    credPtr = IntPtr.Zero;
                }

                return credStatus;
            });
        }

        public static async Task GetAndSaveCredentialsFromUserAsync() {

        }

        public static async Task RemoveCredentialsAsync() {

        }
    }
}
