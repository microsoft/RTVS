// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class IdleTimeService : IIdleTimeService {
        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> Closing;
    }
}
