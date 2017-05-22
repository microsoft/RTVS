// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;
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
            bool result = true;

            vsHierarchy = null;
            vsItemId = (uint)VSConstants.VSITEMID.Nil;

            IVsUIShellOpenDocument vsUIShellOpenDocument = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            IOleServiceProvider serviceProviderUnused = null;
            int docInProject = 0;
            IVsUIHierarchy uiHier = null;


            int hr = vsUIShellOpenDocument.IsDocumentInAProject(filePath, out uiHier, out vsItemId, out serviceProviderUnused, out docInProject);
            if (ErrorHandler.Succeeded(hr) && uiHier != null) {
                vsHierarchy = uiHier as IVsHierarchy;
            } else {
                vsHierarchy = null;
                vsItemId = (uint)VSConstants.VSITEMID.Nil;
                result = false;
            }

            return result;
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
    }
}
