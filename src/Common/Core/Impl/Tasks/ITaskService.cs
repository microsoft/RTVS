// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Tasks {
    public interface ITaskService {
        bool Wait(Func<Task> method, int ms = Timeout.Infinite, CancellationToken cancellationToken = default(CancellationToken));
        bool Wait<T>(Func<Task<T>> method, out T result, int ms = Timeout.Infinite, CancellationToken cancellationToken = default(CancellationToken));
    }
}
