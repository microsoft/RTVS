// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.QuickInfo {
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("R ToolTip QuickInfo Controller")]
    [ContentType(RContentTypeDefinition.ContentType)]
    sealed class QuickInfoControllerProvider : IIntellisenseControllerProvider {
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public QuickInfoControllerProvider(IQuickInfoBroker quickInfoBroker, ICoreShell shell) {
            _quickInfoBroker = quickInfoBroker;
            _shell = shell;
        }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers) {
            QuickInfoController quickInfoController = ServiceManager.GetService<QuickInfoController>(textView);
            if (quickInfoController == null) {
                quickInfoController = new QuickInfoController(textView, subjectBuffers, _quickInfoBroker, _shell);
            }

            return quickInfoController;
        }
    }
}
