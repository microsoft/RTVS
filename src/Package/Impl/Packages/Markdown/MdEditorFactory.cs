// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Editors;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [Guid(MdGuidList.MdEditorFactoryGuidString)]
    internal sealed class MdEditorFactory : BaseEditorFactory {
        public MdEditorFactory(Shell.Package package, IServiceContainer services) :
            base(package, services, MdGuidList.MdEditorFactoryGuid, MdGuidList.MdLanguageServiceGuid) { }
    }
}
