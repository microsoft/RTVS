// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using static System.FormattableString;
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

        public static EnvDTE.Project GetActiveProject() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            var projects = (Array)dte.ActiveSolutionProjects;
            if (projects != null && projects.Length == 1) {
                return (EnvDTE.Project)projects.GetValue(0);
            }
            return null;
        }

        public static void AddNewItem(string templateName, string name, string extension, string projectPath) {
            var project = GetActiveProject();
            if (project != null) {
                DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
                var solution = (Solution2)dte.Solution;

                var compressedTemplateName = Path.ChangeExtension(templateName, "zip");
                var templatePath = Path.Combine(GetProjectItemTemplatesFolder(), compressedTemplateName);

                var uncompressedTemplateFolder = Path.Combine(Path.GetTempPath(), templateName);
                var uncompressedTemplateName = Path.ChangeExtension(compressedTemplateName, "vstemplate");
                var tempTemplateFile = Path.Combine(uncompressedTemplateFolder, uncompressedTemplateName);

                using (ZipArchive zip = ZipFile.OpenRead(templatePath)) {
                    zip.ExtractToDirectory(uncompressedTemplateFolder);
                    var fileName = GetUniqueFileName(projectPath, name, extension);
                    project.ProjectItems.AddFromTemplate(tempTemplateFile, Path.GetFileName(fileName));
                }
            }
        }

        public static string GetUniqueFileName(string folder, string prefix, string extension) {
            string name = Path.ChangeExtension(Path.Combine(folder, prefix), extension);
            if (!File.Exists(name)) {
                return name;
            }

            for (int i = 1; ; i++) {
                name = Path.Combine(folder, Invariant($"{prefix}{i}.{extension}"));
                if (!File.Exists(name)) {
                    return name;
                }
            }
        }

        public static string GetProjectItemTemplatesFolder() {
            string assemblyPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
            return Path.Combine(Path.GetDirectoryName(assemblyPath), @"ItemTemplates\");
        }
    }
}
