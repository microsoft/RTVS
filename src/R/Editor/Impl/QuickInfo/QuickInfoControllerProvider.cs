// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    sealed class QuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        private IQuickInfoBroker quickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            QuickInfoController quickInfoController = ServiceManager.GetService<QuickInfoController>(textView);
            if (quickInfoController == null)
            {
                quickInfoController = new QuickInfoController(textView, subjectBuffers, quickInfoBroker);
            }

            return quickInfoController;
        }
    }
}
