// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    public class RAfterRequestEventArgs : EventArgs {
        public IReadOnlyList<IRContext> Contexts { get; }
        public string Prompt { get; }
        public string Request { get; }
        public bool AddToHistory { get; }
        public bool IsVisible { get; }

        public RAfterRequestEventArgs(IReadOnlyList<IRContext> contexts, string prompt, string request, bool addToHistory, bool isVisible) {
            Contexts = contexts;
            Prompt = prompt;
            Request = request;
            AddToHistory = addToHistory;
            IsVisible = isVisible;
        }
    }
}