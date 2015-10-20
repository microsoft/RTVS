using System;
using Microsoft.Build.Construction;
using Microsoft.R.Actions.Logging;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Logging {
    internal static class FileSystemMirroringProjectLoggingExtensions {
        public static void ApplyProjectChangesStarted(this IActionLog log) {
            log.WriteLineAsync(MessageCategory.General, "Starting applying changes to file-mirroring project");
        }

        public static void ApplyProjectChangesFinished(this IActionLog log) {
            log.WriteLineAsync(MessageCategory.General, "Finished applying changes to file-mirroring project");
        }

        public static void MsBuildAfterChangesApplied(this IActionLog log, ProjectRootElement rootElement) {
            log.WriteLineAsync(MessageCategory.General, "File mirroring project after changes applied:" + Environment.NewLine + rootElement.RawXml);
        }
    }
}