// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.UI;

namespace Microsoft.Common.Core.Shell {
    public static class ShellExtensions {
        public static IProgressDialog GetProgressDialog(this ICoreShell shell) => shell.Services.GetService<IProgressDialog>();
        public static IFileDialog GetFileDialog(this ICoreShell shell) => shell.Services.GetService<IFileDialog>();
    }
}
