// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using Microsoft.R.LanguageServer.Server;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    [JsonRpcScope(MethodPrefix = "r/")]
    public sealed class RSessionService : LanguageServiceBase {
        private readonly IREvalSession _session;

        public RSessionService() {
            _session = Services.GetService<IREvalSession>();
        }

        [JsonRpcMethod]
        public Task<string> execute(string code) 
            => _session.ExecuteCodeAsync(code, CancellationToken.None);

        [JsonRpcMethod]
        public Task interrupt() => _session.InterruptAsync();

        [JsonRpcMethod]
        public Task reset() => _session.ResetAsync();

    }
}
