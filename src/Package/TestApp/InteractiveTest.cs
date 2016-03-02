// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;

namespace Microsoft.VisualStudio.R.Interactive.Test {
    public class InteractiveTest {
        public static void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    IdleTime.DoIdle();
                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}
