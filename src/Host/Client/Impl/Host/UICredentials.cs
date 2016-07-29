// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Text;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class UICredentials : ICredentials {
        public NetworkCredential GetCredential(Uri uri, string authType) {
            var userName = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var password = new StringBuilder(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH + 1);

            // TODO: figure out how to actually make login info persist. CREDUI_FLAGS_EXPECT_CONFIRMATION and CredUIConfirmCredentials?
            bool save = false;
            int err = NativeMethods.CredUIPromptForCredentials(
                IntPtr.Zero, uri.Host.ToString(), IntPtr.Zero, 0, userName, userName.Capacity, password, password.Capacity, ref save,
                NativeMethods.CREDUI_FLAGS_EXCLUDE_CERTIFICATES/* | NativeMethods.CREDUI_FLAGS_DO_NOT_PERSIST*/);
            if (err != 0) {
                throw new UnauthorizedAccessException("No credentials entered.");
            }

            return new NetworkCredential(userName.ToString(), password.ToString());
        }
    }
}
