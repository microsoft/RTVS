// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LanguageServer.VsCode.Contracts.Client;

namespace Microsoft.R.LanguageServer.Client {
    internal sealed class VsCodeClient: IVsCodeClient {
        private readonly ClientProxy _client;
        public VsCodeClient(ClientProxy client) {
            _client = client;
        }
        public IClient Client => _client.Client;
        public ITextDocument TextDocument => _client.TextDocument;
    }
}
