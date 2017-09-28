// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.LanguageServer.Commands {
    internal sealed class GetInterpreterPathCommand : ICommand {
        public const string Name = "r.getInterpreterPath";

        public object Execute(IServiceContainer services, params object[] args) {
            var provider = services.GetService<IRInteractiveWorkflowProvider>();
            var workflow = provider.GetOrCreate();
            return Path.Combine(workflow.RSessions.Broker.ConnectionInfo.Uri.ToString(), "bin" + Path.DirectorySeparatorChar + "x64" + Path.DirectorySeparatorChar);
        }
    }
}
