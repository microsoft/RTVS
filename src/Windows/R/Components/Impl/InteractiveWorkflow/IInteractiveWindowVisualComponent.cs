// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.View;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IInteractiveWindowVisualComponent : IVisualComponent {
        bool IsRunning { get; }
        IWpfTextView TextView { get; }
        ITextBuffer CurrentLanguageBuffer { get; }

        IInteractiveWindow InteractiveWindow { get; }
    }
}