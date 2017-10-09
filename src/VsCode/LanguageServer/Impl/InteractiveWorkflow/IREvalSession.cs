// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    internal interface IREvalSession {
        Task<string> ExecuteCodeAsync(string code, CancellationToken ct);
        Task InterruptAsync();
        Task ResetAsync();
        Task CancelAsync();
    }
}
