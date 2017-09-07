// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System;
using System.Threading.Tasks;
using JsonRpc.Standard;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using LanguageServer.VsCode.Contracts;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.LanguageServer.Server {
    public sealed class InitializaionService : LanguageServiceBase {

        [JsonRpcMethod(AllowExtensionData = true)]
        public InitializeResult Initialize(int processId, Uri rootUri, ClientCapabilities capabilities,
            JToken initializationOptions = null, string trace = null) {
            return new InitializeResult(new ServerCapabilities {
                HoverProvider = true,
                SignatureHelpProvider = new SignatureHelpOptions("()"),
                CompletionProvider = new CompletionOptions(true, new [] {'$', '@'}),
                TextDocumentSync = new TextDocumentSyncOptions {
                    OpenClose = true,
                    WillSave = true,
                    Change = TextDocumentSyncKind.Full
                }
            });
        }

        [JsonRpcMethod(IsNotification = true)]
        public async Task Initialized() {
            await Client.Window.ShowMessage(MessageType.Info, "Hello from language server.");
        }

        [JsonRpcMethod]
        public void Shutdown() {

        }

        [JsonRpcMethod(IsNotification = true)]
        public void Exit() {
            Session.StopServer();
        }

        [JsonRpcMethod("$/cancelRequest", IsNotification = true)]
        public void CancelRequest(MessageId id) {
        }
    }
}
