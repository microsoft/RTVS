// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class SessionUtilities {
        public static IRSession GetInteractiveSession() {
            var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var interactiveWorkflow = workflowProvider.GetOrCreate();
            return interactiveWorkflow.RSession;
        }

        public static async Task<string> GetRWorkingDirectoryAsync() {
            var session = GetInteractiveSession();
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    using (var evaluation = await session.BeginEvaluationAsync(false)) {
                        return await evaluation.GetWorkingDirectory();
                    }
                } catch (TaskCanceledException) { }
            }
            return null;
        }

        public static async Task<string> GetRUserDirectoryAsync() {
            var session = GetInteractiveSession();
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    using (var evaluation = await session.BeginEvaluationAsync(false)) {
                        return await evaluation.GetRUserDirectory();
                    }
                } catch (TaskCanceledException) { }
            }
            return null;
        }

        public static async Task<string> GetFriendlyDirectoryNameAsync(string directory) {
            var userDirectory = await GetRUserDirectoryAsync();
            if (!string.IsNullOrEmpty(userDirectory)) {
                if (directory.StartsWithIgnoreCase(userDirectory)) {
                    var relativePath = PathHelper.MakeRelative(userDirectory, directory);
                    if (relativePath.Length > 0) {
                        return "~/" + relativePath.Replace('\\', '/');
                    }
                    return "~";
                }
                return directory.Replace('\\', '/');
            }
            return directory;
        }
    }
}
