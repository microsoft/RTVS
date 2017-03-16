// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal static class IVsHierarchyExtensions {
        /// <summary>
        /// Returns EnvDTE.Project object for the hierarchy
        /// </summary>
        public static EnvDTE.Project GetDTEProject(this IVsHierarchy hierarchy) {
            VsAppShell.Current.AssertIsOnMainThread();
            object extObject;
            if (ErrorHandler.Succeeded(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject))) {
                return extObject as EnvDTE.Project;
            }
            return null;
        }

        /// <summary>
        /// Convenient way to get to the UnconfiguredProject from the hierarchy
        /// </summary>
        public static UnconfiguredProject GetUnconfiguredProject(this IVsHierarchy hierarchy) {
            return hierarchy.GetBrowseObjectContext()?.UnconfiguredProject;
        }

        /// <summary>
        /// Convenient way to get to the ConfiguredProject from the hierarchy
        /// </summary>
        public static ConfiguredProject GetConfiguredProject(this IVsHierarchy hierarchy) {
            return hierarchy.GetBrowseObjectContext()?.UnconfiguredProject?.LoadedConfiguredProjects?.FirstOrDefault();
        }

        /// <summary>
        /// Convenient way to get to the UnconfiguredProject from the hierarchy
        /// </summary>
        public static IVsBrowseObjectContext GetBrowseObjectContext(this IVsHierarchy hierarchy) {
            VsAppShell.Current.AssertIsOnMainThread();
            var context = hierarchy as IVsBrowseObjectContext;
            if (context == null) {
                var dteProject = hierarchy.GetDTEProject();
                context = dteProject?.Object as IVsBrowseObjectContext;
            }
            return context;
        }
    }
}
