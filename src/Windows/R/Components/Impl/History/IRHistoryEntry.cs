// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    public interface IRHistoryEntry {
        ITrackingSpan EntrySpan { get; }
        ITrackingSpan Span { get; }
        bool IsSelected { get; set; }

        IRHistoryEntry Next { get; }
        IRHistoryEntry Previous { get; }
    }
}