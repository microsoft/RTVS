// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IActiveRInteractiveWindowTracker {
        IInteractiveWindowVisualComponent LastActiveWindow { get; }
        bool IsActive { get; }
    }
}