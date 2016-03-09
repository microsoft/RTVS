// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public sealed class ActiveRInteractiveWindowTrackerMock : IActiveRInteractiveWindowTracker {
        public bool IsActive { get; set; }

        public IInteractiveWindowVisualComponent LastActiveWindow { get; set; }

#pragma warning disable 67
        public event EventHandler<InteractiveWindowChangedEventArgs> LastActiveWindowChanged;
    }
}
