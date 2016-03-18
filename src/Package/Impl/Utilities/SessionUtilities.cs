// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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

        public static async Task<string> GetRWorkingDirectoryAsync(IRInteractiveWorkflow workflow = null) {
            var session = workflow != null ? workflow.RSession : GetInteractiveSession();
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

        public static async Task<string> GetRUserDirectoryAsync(IRInteractiveWorkflow workflow = null) {
            var session = workflow != null ? workflow.RSession : GetInteractiveSession();
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

        public static string GetRShortenedPathName(string name, string userDirectory) {
            if (!string.IsNullOrEmpty(userDirectory)) {
                if (name.StartsWithIgnoreCase(userDirectory)) {
                    var relativePath = PathHelper.MakeRelative(userDirectory, name);
                    if (relativePath.Length > 0) {
                        return "~/" + relativePath.Replace('\\', '/');
                    }
                    return "~";
                }
                return name.Replace('\\', '/');
            }
            return name;
        }

        public static async Task<string> GetRShortenedPathNameAsync(string name) {
            var userDirectory = await GetRUserDirectoryAsync();
            return GetRShortenedPathName(name, userDirectory);
        }

        public static async Task<IEnumerable<string>> GetRShortenedPathNamesAsync(IEnumerable<string> names) {
            var userDirectory = await GetRUserDirectoryAsync();
            var shortenedNames = new List<string>();
            if (!string.IsNullOrEmpty(userDirectory)) {
                foreach (var name in names) {
                    shortenedNames.Add(GetRShortenedPathName(name, userDirectory));
                 }
            }
            return shortenedNames;
        }
    }
}
