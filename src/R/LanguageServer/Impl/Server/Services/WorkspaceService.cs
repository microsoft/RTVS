// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System.Collections.Generic;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;

namespace Microsoft.R.LanguageServer.Server {
    [JsonRpcScope(MethodPrefix = "workspace/")]
    public class WorkspaceService : LanguageServiceBase {
        [JsonRpcMethod(IsNotification = true)]
        public void DidChangeConfiguration(SettingsRoot settings) {
        }

        [JsonRpcMethod(IsNotification = true)]
        public void DidChangeWatchedFiles(ICollection<FileEvent> changes) {
        }
    }
}
