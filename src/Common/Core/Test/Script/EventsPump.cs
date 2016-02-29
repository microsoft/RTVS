// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Script {
    public static class EventsPump {
        public static void DoEvents(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    TestScript.DoEvents();
                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}
