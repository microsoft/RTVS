// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Navigation.Text {
    [Export(typeof(ITextStructureNavigatorProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [ContentType("Interactive Content")]
    internal sealed class TextStructureNavigatorProvider : ITextStructureNavigatorProvider {
        [Import]
        private ITextStructureNavigatorSelectorService NavigatorSelectorService { get; set; }

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        public ITextStructureNavigator CreateTextStructureNavigator(ITextBuffer textBuffer) {
            return new TextStructureNavigator(textBuffer, ContentTypeRegistryService, NavigatorSelectorService);
        }
    }
}
