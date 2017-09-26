using System;
using System.Collections.Generic;
using System.Text;
using LanguageServer.VsCode.Contracts.Client;

namespace Microsoft.R.LanguageServer.Client
{
    internal sealed class VsCodeClient: IVsCodeClient {
        private readonly ClientProxy _client;

        public VsCodeClient(ClientProxy client) {
            _client = client;
        }

        public IClient Client => _client.Client;
        public ITextDocument TextDocument => _client.TextDocument;
    }
}
