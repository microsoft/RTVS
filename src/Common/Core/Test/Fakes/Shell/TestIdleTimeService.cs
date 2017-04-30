// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    internal sealed class TestIdleTimeService: IIdleTimeService, IIdleTimeSource {
        public void DoIdle() {
            UIThreadHelper.Instance.Invoke(() => Idle?.Invoke(null, EventArgs.Empty));
            UIThreadHelper.Instance.DoEvents();
        }

        public event EventHandler<EventArgs> Idle;
#pragma warning disable 67
        public event EventHandler<EventArgs> Closing;
    }
}
