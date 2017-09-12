// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class CoreShell : ICoreShell {
        public CoreShell(IServiceContainer services) {
            Check.ArgumentNull(nameof(services), services);
            Services = services;
        }

        public IServiceContainer Services { get; }
    }
}
