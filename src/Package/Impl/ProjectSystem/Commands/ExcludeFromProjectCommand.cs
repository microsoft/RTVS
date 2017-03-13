// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("1496A755-94DE-11D0-8C3F-00C04FC2AAE2")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class ExcludeFromProjectCommand : ProjectCommand {
        public ExcludeFromProjectCommand(): base((int)VSConstants.VSStd2KCmdID.EXCLUDEFROMPROJECT) { }
    }
}
