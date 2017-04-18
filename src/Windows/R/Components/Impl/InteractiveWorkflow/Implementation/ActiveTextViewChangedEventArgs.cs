// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public class ActiveTextViewChangedEventArgs {
        public IWpfTextView Old { get; }
        public IWpfTextView New { get; }

        public ActiveTextViewChangedEventArgs(IWpfTextView oldValue, IWpfTextView newValue) {
            Old = oldValue;
            New = newValue;
        }
    }
}