// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Tasks;
using Microsoft.R.ExecutionTracing;

namespace Microsoft.R.Debugger.Test {
    internal static class EventTaskSources {
        public static class IRExecutionTracer {
            public static readonly EventTaskSource<Microsoft.R.ExecutionTracing.IRExecutionTracer, RBrowseEventArgs> Browse =
                new EventTaskSource<Microsoft.R.ExecutionTracing.IRExecutionTracer, RBrowseEventArgs>(
                    (o, e) => o.Browse += e,
                    (o, e) => o.Browse -= e);
        }
    }
}
