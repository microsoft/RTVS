// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.ToolWindows;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Commands {
    public class ShowPlotWindowCommand : IAsyncCommandRange {
        private readonly IRPlotManagerVisual _plotManager;
        private readonly IVsUIShell4 _shell;
        private PlotWindowResult[] _windows;

        public ShowPlotWindowCommand(ICoreShell shell, IRInteractiveWorkflow workflow) {
            _plotManager = workflow.Plots as IRPlotManagerVisual;
            _shell = shell.GetService<IVsUIShell4>(typeof(SVsUIShell));
        }

        public CommandStatus GetStatus(int index) {
            _windows = GetSortedWindows();
            if (index >= _windows.Length) {
                return CommandStatus.SupportedAndInvisible;
            }

            return CommandStatus.SupportedAndEnabled;
        }

        public string GetText(int index) {
            if (_windows == null) {
                _windows = GetSortedWindows();
            }

            return _windows[index].Text;
        }

        public Task InvokeAsync(int index) {
            if (index < _windows.Length) {
                ToolWindowUtilities.ShowWindowPane<PlotDeviceWindowPane>(_windows[index].InstanceId, true);
            }

            return Task.CompletedTask;
        }

        public int MaxCount { get; } = 20;

        private PlotWindowResult[] GetSortedWindows() {
            var all = GetAllWindows();
            var assigned = all.Where(w => w.HasDevice).OrderBy(w => w.DeviceNum);
            var unassigned = all.Where(w => !w.HasDevice && w.IsVisible);
            return assigned.Concat(unassigned).ToArray();
        }

        private PlotWindowResult[] GetAllWindows() {
            var instances = new List<PlotWindowResult>();
            try {
                var frames = _shell.EnumerateWindows(
                    __WindowFrameTypeFlags.WINDOWFRAMETYPE_Tool |
                    __WindowFrameTypeFlags.WINDOWFRAMETYPE_AllStates,
                    typeof(PlotDeviceWindowPane).GUID);

                foreach (var frame in frames) {
                    bool isVisible = frame.IsVisible() == VSConstants.S_OK;

                    object num;
                    ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_MultiInstanceToolNum, out num));
                    int instanceId = (int)(uint)num;

                    string text;
                    var visualComponent = _plotManager.GetPlotVisualComponent(instanceId);
                    if (visualComponent != null && visualComponent.Device != null) {
                        if (visualComponent.IsDeviceActive) {
                            text = string.Format(CultureInfo.CurrentUICulture, Resources.PlotWindowCommandActive, visualComponent.Device.DeviceNum);
                        } else {
                            text = string.Format(CultureInfo.CurrentUICulture, Resources.PlotWindowCommand, visualComponent.Device.DeviceNum);
                        }
                    } else {
                        text = Resources.PlotWindowCommandNoDevice;
                    }

                    instances.Add(new PlotWindowResult(instanceId, isVisible, text, visualComponent?.Device != null, visualComponent?.Device?.DeviceNum));
                }
            } catch (Exception) { }
            return instances.ToArray();
        }

        struct PlotWindowResult {
            public PlotWindowResult(int instanceId, bool isVisible, string text, bool hasDevice, int? deviceNum) {
                InstanceId = instanceId;
                IsVisible = isVisible;
                Text = text;
                HasDevice = hasDevice;
                DeviceNum = deviceNum;
            }

            public int InstanceId { get; }
            public bool IsVisible { get; }
            public string Text { get; }
            public bool HasDevice { get; }
            public int? DeviceNum { get; }
        }
    }
}
