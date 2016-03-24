// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionInteraction : IDisposable {
        string Prompt { get; }
        int MaxLength { get; }

        IReadOnlyList<IRContext> Contexts { get; }
        Task RespondAsync(string messageText);
    }
}