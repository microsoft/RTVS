// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.R.Components.Help.Commands {
    public sealed class HelpPreviousCommand : IAsyncCommand {
        private readonly IHelpVisualComponent _component;

        public HelpPreviousCommand(IHelpVisualComponent component) {
            _component = component;
        }

        public CommandStatus Status => _component.Browser != null && _component.Browser.CanGoBack
            ? CommandStatus.SupportedAndEnabled
            : CommandStatus.Supported;

        public Task InvokeAsync() {
            _component.Browser.GoBack();
            return Task.CompletedTask;
        }
    }
}
