// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionExtensions {

        /// <summary>
        /// Schedules function in evaluation queue without waiting. 
        /// </summary>
        /// <param name="session">R Session</param>
        /// <param name="function">Function to scheduel</param>
        public static void ScheduleEvaluation(this IRSession session, Func<IRSessionEvaluation, Task> function) {
            session.GetScheduleEvaluationTask(function).DoNotWait();
        }

        private static async Task GetScheduleEvaluationTask(this IRSession session, Func<IRSessionEvaluation, Task> function) {
            await TaskUtilities.SwitchToBackgroundThread();
            using (var evaluation = await session.BeginEvaluationAsync()) {
                await function(evaluation);
            } 
        }

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

        public static async Task<string> GetRShortenedPathNameAsync(this IRSession session, string name) {
            var userDirectory = await session.GetRUserDirectoryAsync();
            return GetRShortenedPathName(name, userDirectory);
        }

        private static string GetRShortenedPathName(string name, string userDirectory) {
            if (!string.IsNullOrEmpty(userDirectory)) {
                if (name.StartsWithIgnoreCase(userDirectory)) {
                    var relativePath = name.MakeRelativePath(userDirectory);
                    if (relativePath.Length > 0) {
                        return "~/" + relativePath.Replace('\\', '/');
                    }
                    return "~";
                }
                return name.Replace('\\', '/');
            }
            return name;
        }
    }
}