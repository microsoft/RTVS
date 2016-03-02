// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("R QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType(RContentTypeDefinition.ContentType)]
    sealed class QuickInfoSourceProvider : IQuickInfoSourceProvider
    {
         public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(textBuffer);
        }
    }
}
