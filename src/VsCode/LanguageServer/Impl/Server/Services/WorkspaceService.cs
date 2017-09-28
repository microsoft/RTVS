// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System.Collections.Generic;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Microsoft.R.Editor;
using Microsoft.R.LanguageServer.Settings;

namespace Microsoft.R.LanguageServer.Server {
    [JsonRpcScope(MethodPrefix = "workspace/")]
    public sealed class WorkspaceService : LanguageServiceBase {
        /// <summary>
        /// Called by VS Code when configuration (settings) change.
        /// https://github.com/Microsoft/language-server-protocol/blob/master/protocol.md#didchangeconfiguration-notification
        /// </summary>
        /// <param name="settings"></param>
        [JsonRpcMethod(IsNotification = true)]
        public void DidChangeConfiguration(SettingsRoot settings) {
            var es = Services.GetService<IREditorSettings>();

            var e = settings.R.Editor;
            es.FormatScope = e.FormatScope;
            es.FormatOptions.BreakMultipleStatements = e.BreakMultipleStatements;
            es.FormatOptions.IndentSize = e.TabSize;
            es.FormatOptions.TabSize = e.TabSize;
            es.FormatOptions.SpaceAfterKeyword = e.SpaceAfterKeyword;
            es.FormatOptions.SpaceBeforeCurly = e.SpaceBeforeCurly;
            es.FormatOptions.SpacesAroundEquals = e.SpacesAroundEquals;

            es.LintOptions = settings.R.Linting;
        }

        [JsonRpcMethod(IsNotification = true)]
        public void DidChangeWatchedFiles(ICollection<FileEvent> changes) { }
    }
}
