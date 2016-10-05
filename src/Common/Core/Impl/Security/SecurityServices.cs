// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.Security {
    public static class SecurityServices {
        public static bool GetUserCredentials(string authority, bool showUI, IntPtr applicationWindowHandle, out Credentials credentials) {
            credentials = new Credentials();
            var prompted = false;
            IntPtr passwordStorage = IntPtr.Zero;

            try {
                var userNameBuilder = new StringBuilder(CREDUI_MAX_USERNAME_LENGTH + 1);
                var passwordBuilder = new StringBuilder(CREDUI_MAX_PASSWORD_LENGTH + 1);

                var save = false;
                int flags = CREDUI_FLAGS_EXCLUDE_CERTIFICATES | CREDUI_FLAGS_PERSIST | CREDUI_FLAGS_EXPECT_CONFIRMATION | CREDUI_FLAGS_GENERIC_CREDENTIALS;
                if(showUI) {
                    flags |= CREDUI_FLAGS_ALWAYS_SHOW_UI;
                }
                var credui = new CREDUI_INFO {
                    cbSize = Marshal.SizeOf(typeof(CREDUI_INFO)),
                    hwndParent = applicationWindowHandle,
                };

                // For password, use native memory so it can be securely freed.
                passwordStorage = SecurityUtilities.CreatePasswordBuffer();
                int err = CredUIPromptForCredentials(ref credui, authority, IntPtr.Zero, 0,
                                          userNameBuilder, userNameBuilder.Capacity,
                                          passwordStorage, CREDUI_MAX_PASSWORD_LENGTH, ref save, flags);
                if (err != 0) {
                    throw new OperationCanceledException();
                }

                prompted = true;
                credentials.UserName = userNameBuilder.ToString();
                credentials.Password = SecurityUtilities.SecureStringFromNativeBuffer(passwordStorage);
                credentials.Password.MakeReadOnly();
            } finally {
                if (passwordStorage != IntPtr.Zero) {
                    Marshal.ZeroFreeGlobalAllocUnicode(passwordStorage);
                }
            }
            return prompted;
        }
    }
}
