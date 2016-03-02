// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Microsoft.Languages.Editor.Utility {
    public static class GuardedOperations {
        public static void DispatchInvoke(Action toInvoke, DispatcherPriority priority) {
            Action guardedAction =
                () => {
                    try {
                        toInvoke();
                    } catch (Exception e) {
                        Debug.Assert(false, "Guarded invoke caught exception", e.Message);
                    }
                };

            Dispatcher.CurrentDispatcher.BeginInvoke(guardedAction, priority);
        }
    }
}
