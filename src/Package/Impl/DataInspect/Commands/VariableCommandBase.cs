// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal abstract class VariableCommandBase : IAsyncCommand {
        protected VariableView VariableView { get; }

        protected VariableCommandBase(VariableView variableView) {
            VariableView = variableView;
        }

        public CommandStatus Status {
            get {
                var variable = VariableView.GetCurrentSelectedModel();
                return variable != null && IsEnabled(variable) 
                    ? CommandStatus.SupportedAndEnabled 
                    : CommandStatus.Supported;
            }
        }

        public Task InvokeAsync() {
            var variable = VariableView.GetCurrentSelectedModel();
            return variable == null ? Task.CompletedTask : InvokeAsync(variable);
        }

        protected abstract bool IsEnabled(VariableViewModel variable);

        protected abstract Task InvokeAsync(VariableViewModel variable);
    }
}