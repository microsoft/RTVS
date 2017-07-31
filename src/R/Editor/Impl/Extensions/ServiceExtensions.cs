// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Editor.Data;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor {
    public static class ServiceExtensions {
        public static IServiceManager AddEditorServices(this IServiceManager serviceManager)
            => serviceManager
                .AddService<IntelliSenseRSession>()
                .AddService<FunctionRdDataProvider>()
                .AddService<FunctionIndex>()
                .AddService<PackageIndex>()
                .AddService<WorkspaceVariableProvider>();
    }
}
