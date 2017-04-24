// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class ProjectUtilities {

        public static IVsHierarchy GetHierarchy(this ITextBuffer textBuffer) {
            string filePath = textBuffer.GetFileName();
            IVsHierarchy vsHierarchy = null;
            uint vsItemID = (uint)VSConstants.VSITEMID.Nil;

            return TryGetHierarchy(filePath, out vsHierarchy, out vsItemID) ? vsHierarchy : null;
        }

        public static bool TryGetHierarchy(string filePath, out IVsHierarchy vsHierarchy, out uint vsItemId) {
            var vsUIShellOpenDocument = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            IOleServiceProvider serviceProviderUnused;
            IVsUIHierarchy uiHier;

            if (vsUIShellOpenDocument.TryGetUiHierarchy(filePath, out uiHier, out vsItemId, out serviceProviderUnused)) {
                vsHierarchy = uiHier;
                return true;
            }

            vsHierarchy = null;
            return false;
        }

        public static UnconfiguredProject GetUnconfiguredProject(this ITextBuffer textBuffer) {
            string filePath = textBuffer.GetFileName();
            IVsHierarchy vsHierarchy;
            uint vsItemID;
            TryGetHierarchy(filePath, out vsHierarchy, out vsItemID);
            return vsHierarchy?.GetUnconfiguredProject();
        }

        public static ConfiguredProject GetConfiguredProject(this ITextBuffer textBuffer) {
            string filePath = textBuffer.GetFileName();
            IVsHierarchy vsHierarchy;
            uint vsItemID;
            TryGetHierarchy(filePath, out vsHierarchy, out vsItemID);
            return vsHierarchy?.GetConfiguredProject();
        }

        public static async Task<string> GetRemotePathAsync(this ConfiguredProject configuredProject, string localPath) {
            var properties = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            var projectDir = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
            var projectName = properties.GetProjectName();
            var remotePath = await properties.GetRemoteProjectPathAsync();
            return localPath.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName);
        }
    }
}
