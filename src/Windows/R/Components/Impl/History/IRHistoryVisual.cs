// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History {
    public interface IRHistoryVisual : IRHistory {
        IRHistoryWindowVisualComponent VisualComponent { get; }

        IRHistoryWindowVisualComponent GetOrCreateVisualComponent(IRHistoryVisualComponentContainerFactory componentContainerFactory, int instanceId = 0);

        void SendSelectedToTextView(ITextView textView);
        IReadOnlyList<SnapshotSpan> GetAllHistoryEntrySpans();
        IReadOnlyList<SnapshotSpan> GetSelectedHistoryEntrySpans();
    }
}