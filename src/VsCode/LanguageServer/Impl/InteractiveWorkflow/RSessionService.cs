// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.LanguageServer.Server;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    [JsonRpcScope(MethodPrefix = "r/")]
    public sealed class RSessionService : LanguageServiceBase {
        [JsonRpcMethod]
        public string execute(string code) {
            var workflow = Services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            try {
                workflow.RSession.ExecuteAsync(code);
            } catch (RException) { } catch (OperationCanceledException) { }
            return "RESULT";
        }

        [JsonRpcMethod]
        public async Task interrupt() {
            var workflow = Services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            try {
                await workflow.Operations.CancelAsync();
                await workflow.RSession.CancelAllAsync();
            } catch (OperationCanceledException) { }
        }

        [JsonRpcMethod]
        public void reset() {
            var workflow = Services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            workflow.Operations.ResetAsync().DoNotWait();
        }
    }
}
