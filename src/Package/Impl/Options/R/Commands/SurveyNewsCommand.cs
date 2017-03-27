// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Options.R.Commands {
    public sealed class SurveyNewsCommand : System.ComponentModel.Design.MenuCommand {
        private static IServiceContainer _services;

        public SurveyNewsCommand(IServiceContainer services) :
            base(OnCommand, new System.ComponentModel.Design.CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSurveyNews)) {
            _services = _services ?? services;
        }

        public async static void OnCommand(object sender, EventArgs args) {
            try {
                var service = _services.GetService<ISurveyNewsService>();
                await service.CheckSurveyNewsAsync(true);
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _services.Log().Write(LogVerbosity.Normal, MessageCategory.Error, "SurveyNewsCommand exception: " + ex.Message);
            }
        }
    }
}
