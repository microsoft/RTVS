// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IActiveWpfTextViewTracker {
        /// <summary>
        /// Last active view of any kind
        /// </summary>
        IWpfTextView LastActiveTextView { get; }

        /// <summary>
        /// Last active view of a particular content type
        /// </summary>
        IWpfTextView GetLastActiveTextView(IContentType contentType);

        /// <summary>
        /// Last active view of a particular content type
        /// </summary>
        IWpfTextView GetLastActiveTextView(string contentType);

        event EventHandler<ActiveTextViewChangedEventArgs> LastActiveTextViewChanged;
    }
}