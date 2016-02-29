// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistoryProvider {
        IRHistory CreateRHistory(IRInteractive rInteractive);
        IRHistory GetAssociatedRHistory(ITextView textView);
        IRHistoryFiltering CreateFiltering(IRHistory history);
        IWpfTextView GetOrCreateTextView(IRHistory history);
    }
}