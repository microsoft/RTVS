// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.DragDrop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Sql.DragDrop {
    [Export(typeof(IDropHandlerProvider))]
    [ContentType("SQL Server Tools")]
    [DropFormat(DataObjectFormats.VSProjectItems)]
    [Name("SqlDropHandlerProvider")]
    [Order(Before = "DefaultFileDropHandler")]
    internal sealed class DropHandlerProvider : IDropHandlerProvider {
        public DropHandlerProvider() { }
        public IDropHandler GetAssociatedDropHandler(IWpfTextView wpfTextView) {
            return new DropHandler(wpfTextView);
        }
    }
}
