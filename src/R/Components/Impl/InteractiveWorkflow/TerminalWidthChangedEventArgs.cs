// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public class TerminalWidthChangedEventArgs : EventArgs {
        public TerminalWidthChangedEventArgs(int newWidth) {
            NewWidth = newWidth;
        }

        public int NewWidth { get; private set; }
    }
}
