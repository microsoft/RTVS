// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Commands {
    public class ShowPlotDeviceCommand : IAsyncCommandRange {
        private readonly IRPlotManager _plotManager;
        private IRPlotDevice[] _devices;
        private IRPlotDevice _active;

        public ShowPlotDeviceCommand(IRInteractiveWorkflow workflow) {
            _plotManager = workflow.Plots;
        }

        public CommandStatus GetStatus(int index) {
            _devices = _plotManager.GetAllDevices();
            _active = _plotManager.ActiveDevice;
            if (index >= _devices.Length) {
                return CommandStatus.SupportedAndInvisible;
            }

            return CommandStatus.SupportedAndEnabled;
        }

        public string GetText(int index) {
            if (_devices == null) {
                _devices = _plotManager.GetAllDevices();
            }

            if (_active == null) {
                _active = _plotManager.ActiveDevice;
            }

            return GetDeviceName(_devices[index]);
        }

        public async Task<CommandResult> InvokeAsync(int index) {
            if (_devices == null) {
                _devices = _plotManager.GetAllDevices();
            }

            if (index < _devices.Length) {
                var device = _devices[index];
                await _plotManager.ShowDeviceAsync(device, true);
            }

            return CommandResult.Executed;
        }

        public int MaxCount { get; } = 20;

        private string GetDeviceName(IRPlotDevice device) {
            return device == _active ? string.Format(CultureInfo.CurrentUICulture, Resources.PlotWindowCommandActive, device.DeviceNum) :
                string.Format(CultureInfo.CurrentUICulture, Resources.PlotWindowCommand, device.DeviceNum);
        }
    }
}
