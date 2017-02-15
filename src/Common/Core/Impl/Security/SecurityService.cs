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
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.Security {
    public class SecurityService : ISecurityService {
        private readonly ICoreShell _coreShell;

        public SecurityService(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public Credentials GetUserCredentials(string authority, string workspaceName, CancellationToken cancellationToken = default(CancellationToken)) {
            _coreShell.AssertIsOnMainThread();

            var credentials = Credentials.ReadSavedCredentials(authority) ?? GetUserCredentials(workspaceName, cancellationToken);
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
            CredUIWinFlags flags = CredUIWinFlags.CREDUIWIN_CHECKBOX;
            // For password, use native memory so it can be securely freed.
            IntPtr passwordStorage = SecurityUtilities.CreatePasswordBuffer();
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

        public bool DeleteUserCredentials(string authority) {
            return CredDelete(authority, CRED_TYPE.GENERIC, 0);
        }
    }
}
