// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SmartIndent
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Smart Indent")]
    internal class SmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return SmartIndenter.Attach(textView);
        }
    }
}
