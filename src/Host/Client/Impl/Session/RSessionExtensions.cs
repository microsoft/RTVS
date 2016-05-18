// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionExtensions {
        public static async Task<string> GetRWorkingDirectoryAsync(this IRSession session) {
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    return await session.GetWorkingDirectoryAsync();
                } catch (RException) {
                } catch (OperationCanceledException) {
                }
            }
            return null;
        }

        public static async Task<string> GetRUserDirectoryAsync(this IRSession session) {
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    return await RSessionEvaluationCommands.GetRUserDirectoryAsync(session);
                } catch (RException) {
                } catch (OperationCanceledException) { }
            }
            return null;
        }

        public static async Task<string> MakeRelativeToRUserDirectoryAsync(this IRSession session, string name) {
            var userDirectory = await session.GetRUserDirectoryAsync();
            return MakeRelativeToUserDirectory(name, userDirectory);
        }

        public static async Task<IEnumerable<string>> MakeRelativeToRUserDirectoryAsync(this IRSession session, IEnumerable<string> names) {
            var userDirectory = await session.GetRUserDirectoryAsync();
            return names.Select(n => MakeRelativeToUserDirectory(n, userDirectory));
        }

        private static string MakeRelativeToUserDirectory(string name, string userDirectory) {
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

        public static Task<string> GetFunctionCodeAsync(this IRSession session, string functionName) {
            return session.EvaluateAsync<string>(Invariant($"paste0(deparse({functionName}), collapse='\n')"), REvaluationKind.Normal);
        }
    }
}