// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.InteractiveWindow.Shell;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IVsInteractiveWindowProvider {
        IVsInteractiveWindow Create(int instanceId);
        void Open(int instanceId, bool focus);
    }
}