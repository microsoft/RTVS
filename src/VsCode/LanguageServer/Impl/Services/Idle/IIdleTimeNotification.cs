// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.LanguageServer.Services {
    /// <summary>
    /// Allows other application areas to notify idle time service
    /// that user activity happened. Helps timer-based implementations
    /// of idle service to avoid firing event when app is actually busy.
    /// </summary>
    internal interface IIdleTimeNotification {
        void NotifyUserActivity();
    }
}
