// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System;
using System.Threading.Tasks;
using JsonRpc.Standard;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.LanguageServer.Server {
    public sealed class InitializaionService : LanguageServiceBase {
        private const string TriggerCharacters = "`:$@_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        [JsonRpcMethod(AllowExtensionData = true)]
        public InitializeResult Initialize(int processId, Uri rootPath,
            ClientCapabilities capabilities,
            JToken initializationOptions = null, 
            string trace = null) {
            return new InitializeResult(new ServerCapabilities {
                HoverProvider = true,
                SignatureHelpProvider = new SignatureHelpOptions("()"),
                CompletionProvider = new CompletionOptions(true, TriggerCharacters),
                TextDocumentSync = new TextDocumentSyncOptions {
                    OpenClose = true,
                    WillSave = true,
                    Change = TextDocumentSyncKind.Incremental
                }
            });
        }

        [JsonRpcMethod(IsNotification = true)]
        public async Task Initialized() {
            await Client.Window.ShowMessage(MessageType.Info, "R language server started.");
        }

        [JsonRpcMethod]
        public void Shutdown() { }

        [JsonRpcMethod(IsNotification = true)]
        public void Exit() => LanguageServerSession.StopServer();

        [JsonRpcMethod("$/cancelRequest", IsNotification = true)]
        public void CancelRequest(MessageId id) { }
    }
}
