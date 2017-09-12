// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Services.Idle {
    internal sealed class IdleTimeManager {
        private static IdleTimeManager _instance;
        private readonly IIdleTimeNotification _idleNotification;

        private IdleTimeManager(IServiceContainer services) {
            Check.ArgumentNull(nameof(services), services);
            _idleNotification = services.GetService< IIdleTimeNotification>();
        }

        public static IdleTimeManager GetOrCreate(IServiceContainer services)
            => _instance ?? (_instance = new IdleTimeManager(services));

        public void NotifyUserActivity() => _idleNotification.NotifyUserActivity();
    }
}
