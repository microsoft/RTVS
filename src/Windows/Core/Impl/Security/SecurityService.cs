// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Core.Security {
    public class SecurityService : ISecurityService {
        private readonly IServiceContainer _services;

        public SecurityService(IServiceContainer services) {
            _services = services;
        }

        public Credentials GetUserCredentials(string authority, string workspaceName) {
            _services.MainThread().CheckAccess();
            return ReadSavedCredentials(authority) ?? PromptForWindowsCredentials(authority, workspaceName);
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
                var platform = _services.GetService<IPlatformServices>();
                if (Win32MessageBox.Show(platform.ApplicationWindowHandle, message,
                    Win32MessageBox.Flags.YesNo | Win32MessageBox.Flags.IconWarning) == Win32MessageBox.Result.Yes) {
                    certificate2.Reset();
                    return true;
                }
            }
            return false;
        }

        public void DeleteCredentials(string authority) {
            if(!NativeMethods.CredDelete(authority, NativeMethods.CRED_TYPE.GENERIC, 0)) {
                var err = Marshal.GetLastWin32Error();
                if(err != NativeMethods.ERROR_NOT_FOUND) {
                    throw new Win32Exception(err);
                }
            }
        }

        public bool DeleteUserCredentials(string authority) => NativeMethods.CredDelete(authority, NativeMethods.CRED_TYPE.GENERIC, 0);

        public string GetUserName(string authority) {
            using (var ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    var credData = ch.GetCredentialData();
                    return credData.UserName;
                }
                return string.Empty;
            }
        }
        
        private Credentials ReadSavedCredentials(string authority) {
            using (var ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    var credData = ch.GetCredentialData();
                    return Credentials.Create(credData.UserName, SecurityUtilities.SecureStringFromNativeBuffer(credData.CredentialBlob));
                }
                return null;
            }
        }

        private Credentials PromptForWindowsCredentials(string authority, string workspaceName) {
            var credui = new NativeMethods.CREDUI_INFO {
                cbSize = Marshal.SizeOf(typeof(NativeMethods.CREDUI_INFO)),
                hwndParent = _services.GetService<IPlatformServices>().ApplicationWindowHandle,
                pszCaptionText = Resources.Info_ConnectingTo.FormatInvariant(workspaceName)
            };

            uint authPkg = 0;
            var credStorage = IntPtr.Zero;
            var save = true;
            var flags = NativeMethods.CredUIWinFlags.CREDUIWIN_CHECKBOX;
            // For password, use native memory so it can be securely freed.
            var passwordStorage = CreatePasswordBuffer();
            var inCredSize = 1024;
            var inCredBuffer = Marshal.AllocCoTaskMem(inCredSize);

            try {
                if (!NativeMethods.CredPackAuthenticationBuffer(0, WindowsIdentity.GetCurrent().Name, "", inCredBuffer, ref inCredSize)) {
                    var error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error);
                }

                var err = NativeMethods.CredUIPromptForWindowsCredentials(ref credui, 0, ref authPkg, inCredBuffer, (uint)inCredSize, out credStorage, out var credSize, ref save, flags);
                if (err != 0) {
                    throw new OperationCanceledException();
                }

                var userNameBuilder = new StringBuilder(NativeMethods.CRED_MAX_USERNAME_LENGTH);
                var userNameLen = NativeMethods.CRED_MAX_USERNAME_LENGTH;
                var domainBuilder = new StringBuilder(NativeMethods.CRED_MAX_USERNAME_LENGTH);
                var domainLen = NativeMethods.CRED_MAX_USERNAME_LENGTH;
                var passLen = NativeMethods.CREDUI_MAX_PASSWORD_LENGTH;
                if (!NativeMethods.CredUnPackAuthenticationBuffer(NativeMethods.CRED_PACK_PROTECTED_CREDENTIALS, credStorage, credSize, userNameBuilder, ref userNameLen, domainBuilder, ref domainLen, passwordStorage, ref passLen)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                var userName = userNameBuilder.ToString();
                var password = SecurityUtilities.SecureStringFromNativeBuffer(passwordStorage);
                return Save(userName, password, authority, save);
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
        
        private static Credentials Save(string userName, SecureString password, string authority, bool save) {
            var creds = default(NativeMethods.CredentialData);
            try {
                creds.TargetName = authority;
                // We have to save the credentials even if user selected NOT to save. Otherwise, user will be asked to enter
                // credentials for every REPL/intellisense/package/Connection test request. This can provide the best user experience.
                // We can limit how long the information is saved, in the case whee user selected not to save the credential persistence
                // is limited to the current log on session. The credentials will not be available if the use logs off and back on.
                creds.Persist = save ? NativeMethods.CRED_PERSIST.CRED_PERSIST_ENTERPRISE : NativeMethods.CRED_PERSIST.CRED_PERSIST_SESSION;
                creds.Type = NativeMethods.CRED_TYPE.GENERIC;
                creds.UserName = userName;
                creds.CredentialBlob = Marshal.SecureStringToCoTaskMemUnicode(password);
                creds.CredentialBlobSize = (uint)((password.Length + 1) * sizeof(char)); // unicode password + unicode null
                if (!NativeMethods.CredWrite(ref creds, 0)) {
                    var error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, Resources.Error_CredWriteFailed);
                }
            } finally {
                if (creds.CredentialBlob != IntPtr.Zero) {
                    Marshal.ZeroFreeCoTaskMemUnicode(creds.CredentialBlob);
                }
            }
            return Credentials.Create(userName, password);
        }

        private static IntPtr CreatePasswordBuffer() 
            => Marshal.AllocCoTaskMem(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH);
    }
}
