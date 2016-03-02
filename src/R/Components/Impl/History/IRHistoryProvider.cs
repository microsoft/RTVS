﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History {
    public interface IRHistoryProvider {
        IRHistory CreateRHistory(IRInteractiveWorkflow interactiveWorkflow);
        IRHistory GetAssociatedRHistory(ITextBuffer textBuffer);
        IRHistory GetAssociatedRHistory(ITextView textView);
        IRHistoryFiltering CreateFiltering(IRHistoryWindowVisualComponent visualComponent);
    }
}