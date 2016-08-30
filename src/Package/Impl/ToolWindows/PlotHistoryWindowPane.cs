// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Plots.Implementation.Commands;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.Settings;
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
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly int _instanceId;
        private IOleCommandTarget _commandTarget;

        public const string WindowGuidString = "336D81C9-CE7F-4405-A7B5-A3A658EA5050";

        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public PlotHistoryWindowPane(IRPlotManager plotManager, int instanceId, IRSettings settings, ICoreShell coreShell) {
            _plotManager = plotManager;
            _instanceId = instanceId;
            _settings = settings;
            _coreShell = coreShell;

            // this value matches with icmdShowPlotWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.ChartFilter;
            Caption = Resources.PlotHistoryWindowCaption;
            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotHistoryWindowToolBarId);
        }

        protected override void OnCreate() {
            var controller = new AsyncCommandController();
            var visualComponent = new RPlotHistoryVisualComponent(_plotManager, controller, this, _coreShell);
            var commands = new RPlotHistoryCommands(_plotManager.InteractiveWorkflow, visualComponent);

            controller.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, commands.Copy);
            controller.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Cut, commands.Cut);
            controller.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete, commands.Remove);
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryActivatePlot, commands.ActivatePlot);
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryZoomIn, commands.ZoomIn);
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryZoomOut, commands.ZoomOut);
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotHistoryAutoHide, commands.AutoHide);
            Component = visualComponent;
            _commandTarget = new CommandTargetToOleShim(null, Component.Controller);
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

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return _commandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return _commandTarget.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}
