// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controllers.Commands {
    [ExcludeFromCodeCoverage]
    public class EditingCommand : ViewCommand {
        protected IServiceContainer Services { get; }

        public EditingCommand(ITextView textView, Guid group, int id)
            : base(textView, group, id, true) {
        }

        public EditingCommand(ITextView textView, int id)
            : base(textView, Guid.Empty, id, true) {
        }

        public EditingCommand(ITextView textView, IServiceContainer services, CommandId id)
            : base(textView, id, true) {
            Services = services;
        }

        public EditingCommand(ITextView textView, CommandId[] ids)
            : base(textView, ids, true) {
        }
    }
}
