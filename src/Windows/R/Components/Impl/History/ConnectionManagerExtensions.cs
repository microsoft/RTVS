// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.History {
    public static class ConnectionManagerExtensions {
        public static IRHistoryWindowVisualComponent GetOrCreateVisualComponent(this IRHistory cm, IRHistoryVisualComponentContainerFactory componentContainerFactory, int id = 0)
            => ((IRHistoryVisual)cm).GetOrCreateVisualComponent(componentContainerFactory, id);
    }
}
