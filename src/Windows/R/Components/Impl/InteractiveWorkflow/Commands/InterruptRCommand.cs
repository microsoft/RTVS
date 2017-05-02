// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Commands {
    public sealed class InterruptRCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSession _session;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private volatile bool _enabled;

        public InterruptRCommand(IRInteractiveWorkflowVisual interactiveWorkflow, IDebuggerModeTracker debuggerModeTracker) {
            _interactiveWorkflow = interactiveWorkflow;
            _session = interactiveWorkflow.RSession;
            _debuggerModeTracker = debuggerModeTracker;

            _session.Connected += OnConnected;
            _session.Disconnected += OnDisconnected;

            _session.BeforeRequest += OnBeforeRequest;
            _session.AfterRequest += OnAfterRequest;
        }

        private void OnConnected(object sender, RConnectedEventArgs e) => _enabled = true;
        private void OnDisconnected(object sender, EventArgs e) => _enabled = false;

        private void OnBeforeRequest(object sender, RBeforeRequestEventArgs e) {
            _enabled = e.Contexts.Count != 1; // Disable command only if prompt is in the top level
        }

        private void OnAfterRequest(object sender, RAfterRequestEventArgs e) => _enabled = true;

        public CommandStatus Status {
            get {
                var status = CommandStatus.Supported;
                if (_interactiveWorkflow.ActiveWindow == null) {
                    status |= CommandStatus.Invisible;
                } else if (CanInterrupt()) {
                    status |= CommandStatus.Enabled;
                }
                return status;
            }
        }

        public async Task InvokeAsync() {
            if (CanInterrupt()) {
                _interactiveWorkflow.Operations.ClearPendingInputs();
                await _session.CancelAllAsync();
            }
        }

        private bool CanInterrupt()
            => _session.IsHostRunning && _session.IsProcessing && !_debuggerModeTracker.IsInBreakMode;
    }
}
