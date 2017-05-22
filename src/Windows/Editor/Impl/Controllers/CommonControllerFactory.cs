// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Outline;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Controllers {
    [Export(typeof(IControllerFactory))]
    [ContentType("text")]
    [Name("Default")]
    [Order]
    internal class CommonControllerFactory : IControllerFactory {
        private readonly IOutliningManagerService _oms;

        [ImportingConstructor]
        public CommonControllerFactory(ICoreShell shell) {
            _oms = shell.GetService<IOutliningManagerService>();
        }

        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer)
            => new ICommandTarget[] { new OutlineController(textView, _oms) };
    }
}
