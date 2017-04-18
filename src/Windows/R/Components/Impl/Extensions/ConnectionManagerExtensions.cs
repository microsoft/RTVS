// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.ConnectionManager {
    public static class ConnectionManagerExtensions {
        public static IConnectionManagerVisualComponent GetOrCreateVisualComponent(this IConnectionManager cm, int id = 0)
            => ((IConnectionManagerVisual)cm).GetOrCreateVisualComponent(id);
    }
}
