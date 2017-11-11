// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
    public class NullConsole : IConsole {
        public void Write(string text) { }
        public void WriteError(string text) {}
        public Task<bool> PromptYesNoAsync(string text, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}