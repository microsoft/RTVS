// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IConsole {
        void Write(string text);
        Task<bool> PromptYesNoAsync(string text, CancellationToken cancellationToken);
    }
}