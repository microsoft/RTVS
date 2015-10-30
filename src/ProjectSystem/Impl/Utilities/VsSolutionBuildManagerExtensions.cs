using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities {
    public static class VsSolutionBuildManagerExtensions {
        public static string FindActiveProjectCfgName(this IVsSolutionBuildManager5 solutionBuildManager, Guid projectId) {
            string projectCfgCanonicalName;
            var hr = solutionBuildManager.FindActiveProjectCfgName(ref projectId, out projectCfgCanonicalName);
            if (hr < 0) {
                Marshal.ThrowExceptionForHR(hr);
            }
            return projectCfgCanonicalName;
        }
    }
}
