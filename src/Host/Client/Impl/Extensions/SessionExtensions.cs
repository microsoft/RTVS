// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Host.Client {
    public static class SessionExtensions {
        public static async Task<string> GetRWorkingDirectoryAsync(this IRSession session) {
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    using (var evaluation = await session.BeginEvaluationAsync(false)) {
                        return await evaluation.GetWorkingDirectory();
                    }
                } catch (OperationCanceledException) { }
            }
            return null;
        }

        public static async Task<string> GetRUserDirectoryAsync(this IRSession session) {
             if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    using (var evaluation = await session.BeginEvaluationAsync(false)) {
                        return await evaluation.GetRUserDirectory();
                    }
                } catch (OperationCanceledException) { }
            }
            return null;
        }
    }
}
