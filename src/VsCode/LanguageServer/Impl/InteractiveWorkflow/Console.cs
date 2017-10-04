// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.R.LanguageServer.Client;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    internal sealed class Console : IConsole {
        private readonly IServiceContainer _services;
        private IVsCodeClient _client;

        private IVsCodeClient Client => _client ?? (_client = _services.GetService<IVsCodeClient>());

        public Console(IServiceContainer services) {
            _services = services;
        }

        public void WriteError(string text) => Client.Window.LogMessage(MessageType.Error, text);
        public void WriteErrorLine(string text) => Client.Window.LogMessage(MessageType.Error, text);
        public void Write(string text) => Client.Window.LogMessage(MessageType.Info, text);
        public void WriteLine(string text) => Client.Window.LogMessage(MessageType.Info, text);
        public Task<bool> PromptYesNoAsync(string text, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
