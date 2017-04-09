// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    public sealed class BrowserForFolderUIEditor: UITypeEditor {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            string currentDirectory = value as string;
            currentDirectory = (currentDirectory != null && !currentDirectory.StartsWithOrdinal("~")) ? currentDirectory : null;
            try {
                currentDirectory = Path.IsPathRooted(currentDirectory) ? currentDirectory : null;
            } catch(ArgumentException) { }
            var result = VsAppShell.Current.FileDialog().ShowBrowseDirectoryDialog(currentDirectory);
            return !string.IsNullOrEmpty(result) ? result : currentDirectory;
        }
    }
}
