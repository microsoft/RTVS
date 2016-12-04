// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.Common.Core.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal class VsTaskService : ITaskService {
        public bool Wait(Task task, CancellationToken cancellationToken = default(CancellationToken), int ms = Timeout.Infinite) 
            => UIThreadReentrancyScope.WaitOnTaskComplete(task, cancellationToken, ms);
    }
}
