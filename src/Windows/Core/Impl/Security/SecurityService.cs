// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Windows.Core.OS;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Windows.Core.Security {
    public class SecurityService : ISecurityService {
        private readonly ICoreShell _coreShell;

        public SecurityService(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public Credentials GetUserCredentials(string authority, string workspaceName, CancellationToken cancellationToken = default(CancellationToken)) {
            _coreShell.AssertIsOnMainThread();

            var credentials = ReadSavedCredentials(authority) ?? GetUserCredentials(workspaceName, cancellationToken);
            return credentials;
        }

        private Credentials GetUserCredentials(string workspaceName, CancellationToken cancellationToken) {
            var credui = new CREDUI_INFO {
                cbSize = Marshal.SizeOf(typeof(CREDUI_INFO)),
                hwndParent = _coreShell.AppConstants.ApplicationWindowHandle,
                pszCaptionText = Resources.Info_ConnectingTo.FormatInvariant(workspaceName)
            };

            uint authPkg = 0;
            IntPtr credStorage = IntPtr.Zero;
            uint credSize;
            bool save = true;
            var flags = CredUIWinFlags.CREDUIWIN_CHECKBOX;
            // For password, use native memory so it can be securely freed.
            IntPtr passwordStorage = CreatePasswordBuffer();
            int inCredSize = 1024;
            IntPtr inCredBuffer = Marshal.AllocCoTaskMem(inCredSize);

            try {
                if (!CredPackAuthenticationBuffer(0, WindowsIdentity.GetCurrent().Name, "", inCredBuffer, ref inCredSize)) {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error);
                }

                var err = CredUIPromptForWindowsCredentials(ref credui, 0, ref authPkg, inCredBuffer, (uint)inCredSize, out credStorage, out credSize, ref save, flags);
                if (err != 0) {
                    throw new OperationCanceledException();
                }

                StringBuilder userNameBuilder = new StringBuilder(CRED_MAX_USERNAME_LENGTH);
                int userNameLen = CRED_MAX_USERNAME_LENGTH;
                StringBuilder domainBuilder = new StringBuilder(CRED_MAX_USERNAME_LENGTH);
                int domainLen = CRED_MAX_USERNAME_LENGTH;
                int passLen = CREDUI_MAX_PASSWORD_LENGTH;
                if (!CredUnPackAuthenticationBuffer(CRED_PACK_PROTECTED_CREDENTIALS, credStorage, credSize, userNameBuilder, ref userNameLen, domainBuilder, ref domainLen, passwordStorage, ref passLen)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return Credentials.CreateCredentials(userNameBuilder.ToString(), SecurityUtilities.SecureStringFromNativeBuffer(passwordStorage), save);
            } finally {
                if (inCredBuffer != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(inCredBuffer);
                }

                if (credStorage != IntPtr.Zero) {
                    Marshal.ZeroFreeCoTaskMemUnicode(credStorage);
                }

                if (passwordStorage != IntPtr.Zero) {
                    Marshal.ZeroFreeCoTaskMemUnicode(passwordStorage);
                }
            }
        }

        public bool ValidateX509Certificate(X509Certificate certificate, string message) {
            var certificate2 = certificate as X509Certificate2;
            Debug.Assert(certificate2 != null);
            if (certificate2 == null || !certificate2.Verify()) {
                // Use native message box here since Win32 can show it from any thread.
                // Parent window must be NULL since otherwise the call hangs since VS 
                // is in modal state due to the progress dialog. Note that native message
                // box appearance is a bit different from VS dialogs and matches OS theme
                // rather than VS fonts and colors.
                if (Win32MessageBox.Show(_coreShell.AppConstants.ApplicationWindowHandle, message, 
                    Win32MessageBox.Flags.YesNo | Win32MessageBox.Flags.IconWarning) == Win32MessageBox.Result.Yes) {
                    certificate2.Reset();
                    return true;
                }
            }
            return false;
        }

        public void DeleteCredentials(string authority) {
            if(!CredDelete(authority, CRED_TYPE.GENERIC, 0)) {
                int err = Marshal.GetLastWin32Error();
                if(err != ERROR_NOT_FOUND) {
                    throw new Win32Exception(err);
                }
            }
        }

        public bool DeleteUserCredentials(string authority) {
            return CredDelete(authority, CRED_TYPE.GENERIC, 0);
        }

        public void Save(Credentials credentials, string authority) {
            if (!credentials.IsSaved()) {
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
                    creds.CredentialBlobSize = (uint)((credentials.Password.Length + 1) * sizeof(char)); // unicode password + unicode null
                    if (!CredWrite(ref creds, 0)) {
                        var error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error, Resources.Error_CredWriteFailed);
                    }
                    //Source = CredentialSource.Saved;
                } finally {
                    if (creds.CredentialBlob != IntPtr.Zero) {
                        Marshal.ZeroFreeCoTaskMemUnicode(creds.CredentialBlob);
                    }
                }
            }
        }

        /// <summary>
        /// Used to obtain credentials from the Credential Manager
        /// </summary>
        public Credentials ReadSavedCredentials(string authority) {
            using (CredentialHandle ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    CredentialData credData = ch.GetCredentialData();
                    return null;//Credentials.Create(credData.UserName, SecurityUtilities.SecureStringFromNativeBuffer(credData.CredentialBlob), CredentialSource.Saved);
                }
                return null;
            }
        }

        public string GetUserName(string authority) {
            using (var ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    NativeMethods.CredentialData credData = ch.GetCredentialData();
                    return credData.UserName;
                }
                return string.Empty;
            }
        }

        private static IntPtr CreatePasswordBuffer() {
            return Marshal.AllocCoTaskMem(CREDUI_MAX_PASSWORD_LENGTH);
        }
    }
}
