// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal interface IRProjectProperties {
        Task<bool> GetResetReplOnRunAsync();
        Task SetResetReplOnRunAsync(bool val);

        Task<string> GetCommandLineArgsAsync();
        Task SetCommandLineArgsAsync(string val);

        Task<string> GetStartupFileAsync();
        Task SetStartupFileAsync(string val);
    }
}
