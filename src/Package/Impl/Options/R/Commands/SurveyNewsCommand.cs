// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    public sealed class SurveyNewsCommand : MenuCommand {
        private static ICoreShell _coreShell;
        public SurveyNewsCommand(ICoreShell coreShell) :
            base(OnCommand, new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSurveyNews)) {
            _coreShell = coreShell;
        }

        public async static void OnCommand(object sender, EventArgs args) {
            try {
                var service = VsAppShell.Current.ExportProvider.GetExportedValue<ISurveyNewsService>();
                await service.CheckSurveyNewsAsync(true);
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _coreShell.Services.Log.Write(LogVerbosity.Normal, MessageCategory.Error, "SurveyNewsCommand exception: " + ex.Message);
            }
        }
    }
}
