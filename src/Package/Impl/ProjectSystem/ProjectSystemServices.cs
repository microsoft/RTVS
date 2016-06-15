// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IProjectSystemServices))]
    internal sealed class ProjectSystemServices: IProjectSystemServices {
        public EnvDTE.Solution GetSolution() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            return dte.Solution;
        }

        public EnvDTE.Project GetActiveProject() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            if (dte.Solution.Projects.Count > 0) {
                try {
                    return dte.Solution?.Projects?.Cast<EnvDTE.Project>()?.First();
                } catch (COMException) { }
            }
            return null;
        }

        public IVsProject GetSelectedProject() {
            var monSel = VsAppShell.Current.GetGlobalService<IVsMonitorSelection>();
            IntPtr hierarchy = IntPtr.Zero, selectionContainer = IntPtr.Zero;
            uint itemid;
            IVsMultiItemSelect ms;

            try {
                if (VSConstants.S_OK == monSel.GetCurrentSelection(out hierarchy, out itemid, out ms, out selectionContainer)) {
                    return Marshal.GetObjectForIUnknown(hierarchy) as IVsProject;
                }
            } finally {
                if (hierarchy != IntPtr.Zero) {
                    Marshal.Release(hierarchy);
                }
                if (selectionContainer != IntPtr.Zero) {
                    Marshal.Release(selectionContainer);
                }
            }
            return null;
        }

        public void AddNewItem(string templateName, string name, string extension, string destinationPath) {
            var project = GetActiveProject();
            if (project != null) {
                DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
                var solution = (Solution2)dte.Solution;

                // Construct name of the compressed template
                var compressedTemplateName = Path.ChangeExtension(templateName, "zip");
                var templatePath = Path.Combine(GetProjectItemTemplatesFolder(), compressedTemplateName);

                // We will be extracting template contents into a temp folder
                var uncompressedTemplateFolder = Path.Combine(Path.GetTempPath(), templateName);
                var uncompressedTemplateName = Path.ChangeExtension(compressedTemplateName, "vstemplate");
                var tempTemplateFile = Path.Combine(uncompressedTemplateFolder, uncompressedTemplateName);

                // Extract template files overwriting any existing ones
                using (ZipArchive zip = ZipFile.OpenRead(templatePath)) {
                    foreach (ZipArchiveEntry entry in zip.Entries) {
                        if (!Directory.Exists(uncompressedTemplateFolder)) {
                            Directory.CreateDirectory(uncompressedTemplateFolder);
                        }
                        string destFilePath = Path.Combine(uncompressedTemplateFolder, entry.FullName);
                        if (!string.IsNullOrEmpty(entry.Name)) {
                            entry.ExtractToFile(destFilePath, true);
                        } else {
                            Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                        }
                    }

                    // Given path to the project or a folder in it, generate unique file name
                    var fileName = GetUniqueFileName(destinationPath, name, extension);

                    // Locate folder in the project
                    var projectFolder = Path.GetDirectoryName(project.FullName);
                    if (destinationPath.StartsWith(projectFolder, StringComparison.OrdinalIgnoreCase)) {
                        ProjectItems projectItems = project.ProjectItems;

                        if (destinationPath.Length > projectFolder.Length) {
                            var relativePath = destinationPath.Substring(projectFolder.Length + 1);

                            // Go into folders and find project item to insert the file in
                            while (relativePath.Length > 0) {
                                int index = relativePath.IndexOf('\\');
                                string folder;
                                if (index >= 0) {
                                    folder = relativePath.Substring(0, index);
                                    relativePath = relativePath.Substring(index + 1);
                                } else {
                                    folder = relativePath;
                                    relativePath = string.Empty;
                                }
                                try {
                                    var item = projectItems.Item(folder);
                                    projectItems = item.ProjectItems;
                                } catch (COMException) {
                                    return;
                                }
                            }
                        }
                        projectItems?.AddFromTemplate(tempTemplateFile, Path.GetFileName(fileName));
                    }
                }
            }
        }

        public string GetUniqueFileName(string folder, string prefix, string extension, bool appendUnderscore = false) {
            string suffix = appendUnderscore ? "_" : string.Empty;
            string name = Path.ChangeExtension(Path.Combine(folder, prefix), extension);
            if (!File.Exists(name)) {
                return name;
            }

            for (int i = 1; ; i++) {
                name = Path.Combine(folder, Invariant($"{prefix}{suffix}{i}.{extension}"));
                if (!File.Exists(name)) {
                    return name;
                }
            }
        }

        public string GetProjectItemTemplatesFolder() {
            // In F5 (Experimental instance) scenario templates are deployed where the extension is.
            string assemblyPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
            var templatesFolder = Path.Combine(Path.GetDirectoryName(assemblyPath), @"ItemTemplates\");
            if (!Directory.Exists(templatesFolder)) {
                // Real install scenario, templates are in 
                // C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\ItemTemplates\R
                string vsExecutableFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string vsFolder = Path.GetDirectoryName(vsExecutableFileName);
                templatesFolder = Path.Combine(vsFolder, @"ItemTemplates\R\");
            }
            return templatesFolder;
        }

    }
}
