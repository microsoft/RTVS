// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Microsoft.R.Editor.Functions;
using Microsoft.R.LanguageServer.Server;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.LanguageServer.Completions {
    [JsonRpcScope(MethodPrefix = "completionItem/")]
    public sealed class CompletionItemService : LanguageServiceBase {
        private IFunctionIndex _functionIndex;
        private IFunctionIndex FunctionIndex => _functionIndex ?? (_functionIndex = Services.GetService<IFunctionIndex>());

        // The request is sent from the client to the server to resolve additional information
        // for a given completion item.
        [JsonRpcMethod(AllowExtensionData = true)]
        public async Task<CompletionItem> Resolve() {
            var item = RequestContext.Request.Parameters.ToObject<CompletionItem>(Utility.CamelCaseJsonSerializer);
            if (item.Kind != CompletionItemKind.Function) {
                return item;
            }
            var info = await FunctionIndex.GetFunctionInfoAsync(item.Label, item.Data.Type == JTokenType.String ? (string)item.Data : null);
            if (info != null) {
                item.Documentation = info.Description;
            }
            return item;
        }
    }
}
