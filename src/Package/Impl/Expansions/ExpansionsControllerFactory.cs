// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    [Export(typeof(IControllerFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Expansions Command Controller")]
    [Order(Before = "Default")]
    internal class ExpansionsControllerFactory : IControllerFactory {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public ExpansionsControllerFactory(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer) {
            var textManager = _coreShell.GetService<IVsTextManager2>(typeof(SVsTextManager));
            textManager.GetExpansionManager(out IVsExpansionManager expansionManager);

            return new List<ICommandTarget> {
                new ExpansionsController(textView, textBuffer, expansionManager, ExpansionsCache.Current, _coreShell.Services)
            };
        }
    }
}
