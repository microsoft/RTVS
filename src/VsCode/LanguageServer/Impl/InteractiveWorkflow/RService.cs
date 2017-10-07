// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using JsonRpc.Standard.Contracts;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.LanguageServer.Server;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    [JsonRpcScope(MethodPrefix = "r/")]
    public sealed class RService : LanguageServiceBase {
        [JsonRpcMethod]
        public string execute(string code) {
            var workflow = Services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            try {
                workflow.RSession.ExecuteAsync(code);
            } catch(RException) { } catch(OperationCanceledException) { }
            return "RESULT";
        }
    }
}
