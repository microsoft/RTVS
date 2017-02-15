// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.Security {
    public sealed class Credentials : ICredentials {
        // Although NetworkCredential does support SecureString, it still exposes
        // plain text password via property and it is visible in a debugger.
        public string UserName { get; set; }
        public SecureString Password { get; set; }

        private CredentialSource Source { get; set; }

        private enum CredentialSource {
            Saved, // Obtained from Credential manager
            NewSave, // Obtained from user with save flag set
            NewNoSave // Obtained from user with out save flag set
        }

        public void Save(string authority) {
            if (!IsSaved()) {
                CredentialData creds = default(CredentialData);
                try {
                    creds.TargetName = authority;
                    // We have to save the credentials even if user selected NOT to save. Otherwise, user will be asked to enter
                    // credentials for every REPL/intellisense/package/Connection test request. This can provide the best user experience.
                    // We can limit how long the information is saved, in the case whee user selected not to save the credential persistence
                    // is limited to the current log on session. The credentials will not be available if the use logs off and back on.
                    creds.Persist = CanSave() ? CRED_PERSIST.CRED_PERSIST_ENTERPRISE : CRED_PERSIST.CRED_PERSIST_SESSION;
                    creds.Type = CRED_TYPE.GENERIC;
                    creds.UserName = UserName;
                    creds.CredentialBlob = Marshal.SecureStringToCoTaskMemUnicode(Password);
                    creds.CredentialBlobSize = (uint)((Password.Length + 1) * sizeof(char)); // unicode password + unicode null
                    if (!CredWrite(ref creds, 0)) {
                        var error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error, Resources.Error_CredWriteFailed);
                    }
                    Source = CredentialSource.Saved;
                } finally {
                    if (creds.CredentialBlob != IntPtr.Zero) {
                        Marshal.ZeroFreeCoTaskMemUnicode(creds.CredentialBlob);
                    }
                }
            }
        }

        public bool CanSave() {
            return Source != CredentialSource.NewNoSave;
        }

        public bool IsSaved() {
            return Source == CredentialSource.Saved;
        }

        /// <summary>
        /// Used for credentials obtained from the user via prompt, with 'save'.
        /// </summary>
        public static Credentials CreateCredentials(string userName, SecureString password, bool save = false) {
            return Create(userName, password, save ? CredentialSource.NewSave : CredentialSource.NewNoSave);
        }

        /// <summary>
        /// Used to obtain credentials from the Credential Manager
        /// </summary>
        public static Credentials ReadSavedCredentials(string authority) {
            using (CredentialHandle ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    CredentialData credData = ch.GetCredentialData();
                    return Create(credData.UserName, SecurityUtilities.SecureStringFromNativeBuffer(credData.CredentialBlob), CredentialSource.Saved);
                }
                return null;
            }
        }

        private static Credentials Create(string userName, SecureString password, CredentialSource source) {
            var creds = new Credentials() { UserName = userName, Password = password, Source = source };
            creds.Password.MakeReadOnly();
            return creds;
        }

        public NetworkCredential GetCredential(Uri uri, string authType) {
            return new NetworkCredential(UserName, Password);
        }
    }
}
