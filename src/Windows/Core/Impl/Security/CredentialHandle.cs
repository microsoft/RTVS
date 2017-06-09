// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS;
using Microsoft.Win32.SafeHandles;
using static Microsoft.Common.Core.NativeMethods;
using static System.FormattableString;

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

        internal static CredentialHandle ReadFromCredentialManager(string authority) {
            if (NativeMethods.CredRead(authority, NativeMethods.CRED_TYPE.GENERIC, 0, out IntPtr creds)) {
                return new CredentialHandle(creds);
            }
            var error = Marshal.GetLastWin32Error();
            // if credentials were not found then continue to prompt user for credentials.
            // otherwise there was an error while reading credentials. 
            if (error != ERROR_NOT_FOUND) {
                Win32MessageBox.Show(IntPtr.Zero, Invariant($"{Common.Core.Resources.Error_CredReadFailed} {ErrorCodeConverter.MessageFromErrorCode(error)}"),
                     Win32MessageBox.Flags.OkOnly | Win32MessageBox.Flags.Topmost | Win32MessageBox.Flags.TaskModal);
            }
            return null;
        }
    }
}
