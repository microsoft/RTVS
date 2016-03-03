// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    public interface IPackage: IServiceProvider {
        T GetPackageService<T>(Type t = null) where T : class;
        DialogPage GetDialogPage(Type t);
    }
}
