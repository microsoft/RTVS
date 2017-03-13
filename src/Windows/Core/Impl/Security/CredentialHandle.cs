// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Common.Core.Security {
    internal sealed class CredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid {

        private CredentialHandle(IntPtr credHandle) {
            SetHandle(credHandle);
        }

        protected override bool ReleaseHandle() {
            if (!IsInvalid) {
                NativeMethods.CredFree(handle);
                SetHandleAsInvalid();
                return true;
            }

            return false;
        }

        public NativeMethods.CredentialData GetCredentialData() {
            if (!IsInvalid) {
                return Marshal.PtrToStructure<NativeMethods.CredentialData>(handle);
            }
            throw new InvalidOperationException(Resources.Error_CredentialHandleInvalid);
        }

        public static CredentialHandle ReadFromCredentialManager(string authority) {
            IntPtr creds;
            if (NativeMethods.CredRead(authority, NativeMethods.CRED_TYPE.GENERIC, 0, out creds)) {
                return new CredentialHandle(creds);
            } else {
                var error = Marshal.GetLastWin32Error();
                // if credentials were not found then continue to prompt user for credentials.
                // otherwise there was an error while reading credentials. 
                if (error != NativeMethods.ERROR_NOT_FOUND) {
                    throw new Win32Exception(error, Resources.Error_CredReadFailed);
                }
            }

            return null;
        }
    }
}
