// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controllers.Commands {
    /// <summary>
    /// Command factory is exported via MEF for a given content
    /// type and allows adding commands to controllers
    /// via exports rather than directly in code.
    /// </summary>
    public interface ICommandFactory {
        IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer);
    }
}
