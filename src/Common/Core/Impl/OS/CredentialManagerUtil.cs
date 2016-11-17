// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.OS {
    public class CredentialManagerUtil {
        public static bool DeleteCredentials(string authority) {
            return CredDelete(authority, CRED_TYPE.GENERIC, 0);
        }
    }
}
