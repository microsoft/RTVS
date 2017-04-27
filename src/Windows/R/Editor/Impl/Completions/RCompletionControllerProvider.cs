﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion controller provider for Visual Studio core editor
    /// </summary>
    [Export(typeof(IIntellisenseControllerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Completion Controller")]
    [Order(Before = "Default Completion Controller")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class RCompletionControllerProvider : IIntellisenseControllerProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RCompletionControllerProvider(ICoreShell shell) => _shell = shell;

        public IIntellisenseController TryCreateIntellisenseController(ITextView view, IList<ITextBuffer> subjectBuffers)
            => RCompletionController.Create(view, subjectBuffers, _shell.Services);
    }
}