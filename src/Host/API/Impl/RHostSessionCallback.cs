// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    public class RHostSessionCallback: IRHostSessionCallback {
        public virtual Task ShowErrorMessageAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
            => Task.CompletedTask;

        public virtual Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(MessageButtons.OK);

        public virtual Task PlotAsync(byte[] image)
            => Task.CompletedTask;

        public virtual PlotDeviceProperties PlotDeviceProperties
            => new PlotDeviceProperties(1024, 1024, 96);

        public virtual void Output(string message, bool error) { }
    }
}
