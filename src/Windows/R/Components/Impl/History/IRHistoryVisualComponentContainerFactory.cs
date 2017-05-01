// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    public interface IRHistoryVisualComponentContainerFactory {
        IVisualComponentContainer<IRHistoryWindowVisualComponent> GetOrCreate(ITextBuffer historyTextBuffer, int instanceId = 0);
    }
}