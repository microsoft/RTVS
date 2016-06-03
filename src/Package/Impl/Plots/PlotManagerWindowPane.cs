// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots {
    [Guid(WindowGuidString)]
    internal class PlotManagerWindowPane : VisualComponentToolWindow<IRPlotManagerVisualComponent>, IOleCommandTarget {
        private readonly IRPlotManager _plotManager;
        private readonly IRSession _session;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;

        public const string WindowGuidString = "993DB88E-ED56-4503-9704-CA90EDAE505C";

        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public PlotManagerWindowPane(IRPlotManager plotManager, IRSession session, IRSettings settings, ICoreShell coreShell) {
            _plotManager = plotManager;
            _session = session;
            _settings = settings;
            _coreShell = coreShell;

            // this value matches with icmdShowPlotWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.LineChart;
            Caption = Resources.PlotWindowCaption;
            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);
        }

        protected override void OnCreate() {
            Component = new RPlotManagerVisualComponent(_plotManager, this, _session, _settings, _coreShell);
            base.OnCreate();
        }

        protected override void Dispose(bool disposing) {
            if (disposing && Component != null) {
                Component.Dispose();
                Component = null;
            }
            base.Dispose(disposing);
        }
    }
}
