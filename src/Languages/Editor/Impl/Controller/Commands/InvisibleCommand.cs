// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller.Commands {
    [ExcludeFromCodeCoverage]
    public class InvisibleCommand : ViewCommand, ICommand {
        public InvisibleCommand(ITextView textView, Guid group, int id)
            : base(textView, group, id, false) {
        }

        CommandStatus ICommandTarget.Status(Guid group, int id) {
            return CommandStatus.Invisible | CommandStatus.Supported;
        }
    }
}
