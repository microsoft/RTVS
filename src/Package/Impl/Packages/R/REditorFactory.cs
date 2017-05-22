// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Editors;

namespace Microsoft.VisualStudio.R.Packages.R {
    [Guid(RGuidList.REditorFactoryGuidString)]
    internal sealed class REditorFactory : BaseEditorFactory {
        public REditorFactory(Shell.Package package, IServiceContainer services) :
            base(package, services, RGuidList.REditorFactoryGuid, RGuidList.RLanguageServiceGuid) { }
    }
}
