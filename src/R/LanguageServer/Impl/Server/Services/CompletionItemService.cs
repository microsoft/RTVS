// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;

namespace Microsoft.R.LanguageServer.Server {
    [JsonRpcScope(MethodPrefix = "completionItem/")]
    public sealed class CompletionItemService : LanguageServiceBase {
        // The request is sent from the client to the server to resolve additional information
        // for a given completion item.
        [JsonRpcMethod(AllowExtensionData = true)]
        public CompletionItem Resolve() {
            var item = RequestContext.Request.Parameters.ToObject<CompletionItem>(Utility.CamelCaseJsonSerializer);
            // Add a pair of square brackets around the inserted text.
            item.InsertText = $"[{item.Label}]";
            return item;
        }
    }
}
