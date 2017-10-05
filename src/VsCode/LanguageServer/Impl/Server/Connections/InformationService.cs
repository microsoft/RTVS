// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
using JsonRpc.Standard.Contracts;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Platform.Interpreters;

namespace Microsoft.R.LanguageServer.Server {
    [JsonRpcScope(MethodPrefix = "information/")]
    public sealed class InformationService : LanguageServiceBase {
        [JsonRpcMethod]
        public string getInterpreterPath() {
            if(!IsRInstalled()) {
                return null;
            }

            var provider = Services.GetService<IRInteractiveWorkflowProvider>();
            var workflow = provider.GetOrCreate();
            var homePath = workflow.RSessions.Broker.ConnectionInfo.Uri.OriginalString.Replace('/', Path.DirectorySeparatorChar);
            var binPath = $"bin{Path.DirectorySeparatorChar}x64{Path.DirectorySeparatorChar}R.exe";
            return Path.Combine(homePath, binPath);
        }

        private bool IsRInstalled() {
            var ris = Services.GetService<IRInstallationService>();
            var engines = ris
                .GetCompatibleEngines(new SupportedRVersionRange(3, 2, 3, 9))
                .OrderByDescending(x => x.Version)
                .ToList();

            return engines.Count > 0;
        }
    }
}
