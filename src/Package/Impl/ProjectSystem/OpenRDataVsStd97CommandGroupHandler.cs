// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [ExportCommandGroup("5EFC7975-14BC-11CF-9B2B-00AA00573819")]
    [AppliesTo("RTools")]
    internal sealed class OpenRDataVsStd97CommandGroupHandler : OpenRDataCommandGroupHandler {
        [ImportingConstructor]
        public OpenRDataVsStd97CommandGroupHandler(UnconfiguredProject unconfiguredProject, IRSessionProvider sessionProvider)
            : base(unconfiguredProject, sessionProvider, (long)VSConstants.VSStd97CmdID.Open) {}
    }
}