// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Guid(WindowGuidString)]
    internal class PlotDeviceWindowPane : VisualComponentToolWindow<IRPlotDeviceVisualComponent>, IOleCommandTarget {
        private readonly IRPlotManagerVisual _plotManager;
        private readonly int _instanceId;
        private IOleCommandTarget _commandTarget;

        public const string WindowGuidString = "993DB88E-ED56-4503-9704-CA90EDAE505C";

        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public PlotDeviceWindowPane(IRPlotManagerVisual plotManager, IRSession session, int instanceId, IServiceContainer services): base(services) {
            _plotManager = plotManager;
            _instanceId = instanceId;

            // this value matches with icmdShowPlotWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.LineChart;
            Caption = Resources.PlotWindowCaption;
            ToolBar = new System.ComponentModel.Design.CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);
        }

        protected override void OnCreate() {
            Component = new RPlotDeviceVisualComponent(_plotManager, _instanceId, this, Services);
            _plotManager.RegisterVisualComponent(Component);

            var commands = new RPlotDeviceCommands(_plotManager.InteractiveWorkflow, Component);
            var controller = new AsyncCommandController()
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdActivatePlotWindow, commands.ActivatePlotDevice)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdClearPlots, commands.RemoveAllPlots)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRemovePlot, commands.RemoveCurrentPlot)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdNextPlot, commands.NextPlot)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPrevPlot, commands.PreviousPlot)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdExportPlotAsImage, commands.ExportAsImage)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdExportPlotAsPdf, commands.ExportAsPdf)
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, commands.Copy)
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Cut, commands.Cut)
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste, commands.Paste)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCopyPlotAsBitmap, commands.CopyAsBitmap)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCopyPlotAsMetafile, commands.CopyAsMetafile)
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdEndLocator, commands.EndLocator);
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
            => _commandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) 
            =>_commandTarget.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
    }
}
