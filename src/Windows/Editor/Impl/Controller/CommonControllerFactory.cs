// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Controller {
    [Export(typeof(IControllerFactory))]
    [ContentType("text")]
    [Name("Default")]
    [Order]
    internal class CommonControllerFactory : IControllerFactory {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public CommonControllerFactory(ICoreShell shell) {
            _shell = shell;
        }

        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer) {
            var list = new List<ICommandTarget>();

            list.Add(new OutlineController(textView, _shell.GetService<IOutliningManagerService>()));
            return list;
        }
    }
}
