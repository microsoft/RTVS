// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.LanguageServer.Commands {
    internal sealed class GetInterpreterPathCommand : ICommand {
        public const string Name = "r.getInterpreterPath";

        public Task<object> ExecuteAsync(IServiceContainer services, params object[] args) {
            var provider = services.GetService<IRInteractiveWorkflowProvider>();
            var workflow = provider.GetOrCreate();
            var homePath = workflow.RSessions.Broker.ConnectionInfo.Uri.OriginalString.Replace('/', Path.DirectorySeparatorChar);
            var binPath = "bin" + Path.DirectorySeparatorChar + "x64" + Path.DirectorySeparatorChar;
            return Task.FromResult((object)Path.Combine(homePath, binPath));
        }
    }
}
