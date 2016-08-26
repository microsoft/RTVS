// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Settings;
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
        private readonly IRPlotManager _plotManager;
        private readonly IRSession _session;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly int _instanceId;
        private IOleCommandTarget _commandTarget;

        public const string WindowGuidString = "993DB88E-ED56-4503-9704-CA90EDAE505C";

        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public PlotDeviceWindowPane(IRPlotManager plotManager, IRSession session, int instanceId, IRSettings settings, ICoreShell coreShell) {
            _plotManager = plotManager;
            _session = session;
            _instanceId = instanceId;
            _settings = settings;
            _coreShell = coreShell;

            // this value matches with icmdShowPlotWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.LineChart;
            Caption = Resources.PlotWindowCaption;
            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);
        }

        protected override void OnCreate() {
            var controller = new AsyncCommandController();
            var viewModel = new RPlotDeviceViewModel(_plotManager, _session, _instanceId);
            //controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdNewPlotWindow, PlotDeviceCommandFactory.NewPlotDevice(_plotManager.InteractiveWorkflow));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdActivatePlotWindow, PlotDeviceCommandFactory.ActivatePlotDevice(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdClearPlots, PlotDeviceCommandFactory.RemoveAllPlots(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRemovePlot, PlotDeviceCommandFactory.RemoveCurrentPlot(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdNextPlot, PlotDeviceCommandFactory.NextPlot(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPrevPlot, PlotDeviceCommandFactory.PreviousPlot(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdExportPlotAsImage, PlotDeviceCommandFactory.ExportAsImage(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdExportPlotAsPdf, PlotDeviceCommandFactory.ExportAsPdf(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, PlotDeviceCommandFactory.CutCopy(_plotManager.InteractiveWorkflow, viewModel, false));
            controller.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Cut, PlotDeviceCommandFactory.CutCopy(_plotManager.InteractiveWorkflow, viewModel, true));
            controller.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste, PlotDeviceCommandFactory.Paste(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCopyPlotAsBitmap, PlotDeviceCommandFactory.CopyAsBitmap(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCopyPlotAsMetafile, PlotDeviceCommandFactory.CopyAsMetafile(_plotManager.InteractiveWorkflow, viewModel));
            controller.AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdEndLocator, PlotDeviceCommandFactory.EndLocator(_plotManager.InteractiveWorkflow, viewModel));
            Component = new RPlotDeviceVisualComponent(_plotManager, controller, viewModel, this, _settings, _coreShell);
            _plotManager.RegisterVisualComponent(Component);
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
