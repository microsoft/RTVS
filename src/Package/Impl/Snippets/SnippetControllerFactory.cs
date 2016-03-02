// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    [Export(typeof(IControllerFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Snippets Command Controller")]
    [Order(Before = "Default")]
    internal class SnippetControllerFactory : IControllerFactory {
        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer) {
            return new List<ICommandTarget>() {
                new SnippetController(textView, textBuffer)
            };
        }
    }
}
