// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Guid(WindowGuidString)]
    internal class PlotHistoryWindowPane : VisualComponentToolWindow<IRPlotHistoryVisualComponent>, IOleCommandTarget {
        private readonly IRPlotManager _plotManager;
        private IOleCommandTarget _commandTarget;

        public const string WindowGuidString = "336D81C9-CE7F-4405-A7B5-A3A658EA5050";

        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public PlotHistoryWindowPane(IRPlotManager plotManager, IServiceContainer services): base(services) {
            _plotManager = plotManager;

            // this value matches with icmdShowPlotWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.ChartFilter;
            Caption = Resources.PlotHistoryWindowCaption;
            ToolBar = new System.ComponentModel.Design.CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotHistoryWindowToolBarId);
        }

        protected override void OnCreate() {
            var visualComponent = new RPlotHistoryVisualComponent(_plotManager, this, Services);
            Component = visualComponent;

            var commands = new RPlotHistoryCommands(_plotManager.InteractiveWorkflow, visualComponent);
            var controller = new AsyncCommandController()
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, commands.Copy)
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Cut, commands.Cut)
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete, commands.Remove)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryActivatePlot, commands.ActivatePlot)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryZoomIn, commands.ZoomIn)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryZoomOut, commands.ZoomOut)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryAutoHide, commands.AutoHide);
            _commandTarget = new CommandTargetToOleShim(null, controller);
            base.OnCreate();
        }

        protected override void Dispose(bool disposing) {
            if (disposing && Component != null) {
                _commandTarget = null;
                Component.Dispose();
                Component = null;
            }
            base.Dispose(disposing);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            =>_commandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            => _commandTarget.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
    }
}
