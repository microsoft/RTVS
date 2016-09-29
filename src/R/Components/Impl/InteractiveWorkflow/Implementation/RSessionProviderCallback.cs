// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    public sealed class RSessionProviderCallback : IRSessionProviderCallback {
        private readonly ICoreShell _coreShell;
        private readonly Lazy<IRInteractiveWorkflow> _workflowLazy;

        public async Task<IntPtr> GetApplicationWindowHandleAsync() {
            await _coreShell.SwitchToMainThreadAsync();
            return _coreShell.AppConstants.ApplicationWindowHandle;
        }

        public RSessionProviderCallback(ICoreShell coreShell, Lazy<IRInteractiveWorkflow> workflowLazy) {
            _coreShell = coreShell;
            _workflowLazy = workflowLazy;
        }

        public void WriteConsole(string text) {
            _coreShell.DispatchOnUIThread(() => _workflowLazy.Value.ActiveWindow?.InteractiveWindow?.WriteErrorLine(text));
        }
    }
}