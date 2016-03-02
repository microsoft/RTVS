// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Controller {
    [Export(typeof(IControllerFactory))]
    [ContentType("text")]
    [Name("Default")]
    [Order]
    internal class CommonControllerFactory : IControllerFactory {
        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer) {
            var list = new List<ICommandTarget>();

            list.Add(new OutlineController(textView));
            return list;
        }
    }
}
