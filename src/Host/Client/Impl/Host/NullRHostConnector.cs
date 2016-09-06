// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class NullRHostConnector : IRHostConnector {
        private static Task<RHost> Result { get; } = TaskUtilities.CreateCanceled<RHost>(
            new RHostDisconnectedException(
                string.Format(CultureInfo.InvariantCulture, Resources.NoConnectionsAvailable, Environment.NewLine, Environment.NewLine)));

        public Uri BrokerUri { get; } = new Uri("http://localhost");
        public bool IsRemote { get; } = true;

        public Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) => Result;

        public void Dispose() { }
    }
}