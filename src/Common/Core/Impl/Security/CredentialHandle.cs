// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Win32.SafeHandles;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core {
    internal sealed class CredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid {

        private CredentialHandle(IntPtr credHandle) {
            SetHandle(credHandle);
        }

        protected override bool ReleaseHandle() {
            if (!IsInvalid) {
                CredFree(handle);
                SetHandleAsInvalid();
                return true;
            }

            return false;
        }

        public CredentialData GetCredentialData() {
            if (!IsInvalid) {
                return Marshal.PtrToStructure<CredentialData>(handle);
            }
            throw new InvalidOperationException(Resources.Error_CredentialHandleInvalid);
        }

        public static CredentialHandle ReadFromCredentialManager(string authority, ICoreShell coreShell) {
            IntPtr creds;
            if (CredRead(authority, CRED_TYPE.GENERIC, 0, out creds)) {
                return new CredentialHandle(creds);
            } else {
                var error = Marshal.GetLastWin32Error();
                // if credentials were not found then continue to prompt user for credentials.
                // otherwise there was an error while reading credentials. 
                if (error != ERROR_NOT_FOUND) {
                    coreShell.ShowErrorMessage(Resources.Error_CredReadFailed + " " + ErrorCodeConverter.MessageFromErrorCode(error));
                }
            }

            return null;
        }
    }
}
