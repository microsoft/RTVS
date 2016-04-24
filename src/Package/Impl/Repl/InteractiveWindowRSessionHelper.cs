// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public static class InteractiveWindowRSessionHelper {
        public static IRSession GetInteractiveWindowRSession(this IRSessionProvider provider) {
            return provider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid);
        }
    }
}
