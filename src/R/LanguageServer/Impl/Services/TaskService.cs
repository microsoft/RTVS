// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class TaskService: ITaskService {
        public bool Wait(Task task, CancellationToken cancellationToken = new CancellationToken(), int ms = Timeout.Infinite)
            => Task.WaitAll(new[] {task}, ms, cancellationToken);
    }
}
