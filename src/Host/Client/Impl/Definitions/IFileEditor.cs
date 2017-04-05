// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Provides functionality for R 'edit' command
    /// </summary>
    public interface IFileEditor {
        Task<string> EditFileAsync(string content, string fileName, CancellationToken ct = default(CancellationToken));
    }
}
