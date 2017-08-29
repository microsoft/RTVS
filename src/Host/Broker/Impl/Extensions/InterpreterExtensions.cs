// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Host.Broker.Interpreters;

namespace Microsoft.R.Host.Broker {
    static class InterpreterExtensions {
        public static Interpreter Latest(this IReadOnlyCollection<Interpreter> interpreters) {
            return interpreters.OrderByDescending(x => x.Version).First();
        }
    }
}
