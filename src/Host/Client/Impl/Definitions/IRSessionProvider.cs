// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        IRSession GetOrCreate(Guid guid);
        IEnumerable<IRSession> GetSessions();
    }
}