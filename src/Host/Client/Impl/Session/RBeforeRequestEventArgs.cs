// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    public class RBeforeRequestEventArgs : EventArgs {
        public IReadOnlyList<IRContext> Contexts { get; }
        public string Prompt { get; }
        public int MaxLength { get; }
        public bool AddToHistoty { get; }

        public RBeforeRequestEventArgs(IReadOnlyList<IRContext> contexts, string prompt, int maxLength, bool addToHistoty) {
            Contexts = contexts;
            Prompt = prompt;
            MaxLength = maxLength;
            AddToHistoty = addToHistoty;
        }
    }
}