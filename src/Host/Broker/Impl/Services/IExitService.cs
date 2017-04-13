// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;

namespace Microsoft.R.Host.Broker.Services {
    public interface IExitService {
        void Exit();
        CancellationToken CancellationToken { get; }
    }
}