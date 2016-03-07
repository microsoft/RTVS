// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    /// <summary>
    /// Editor factory that instead of opening .rproj file in the editor
    /// locates matching .rxproj file, if any, and opens the project instead.
    /// </summary>
    [Guid(RGuidList.RProjEditorFactoryGuidString)]
    internal sealed class RProjEditorFactory : BaseEditorFactory {
        public RProjEditorFactory(Microsoft.VisualStudio.Shell.Package package) :
            base(package, RGuidList.RProjEditorFactoryGuid, RGuidList.RProjLanguageServiceGuid) { }

        public override int CreateEditorInstance(
            uint createEditorFlags,
            string documentMoniker,
            string physicalView,
            IVsHierarchy hierarchy,
            uint itemid,
            IntPtr docDataExisting,
            out IntPtr docView,
            out IntPtr docData,
            out string editorCaption,
            out Guid commandUIGuid,
            out int createDocumentWindowFlags) {

            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            editorCaption = null;
            commandUIGuid = Guid.Empty;
            createDocumentWindowFlags = 0;

            // Change extension to .rxproj
            var rxProjFile = Path.ChangeExtension(documentMoniker, RContentTypeDefinition.VsRProjectExtension);
            if (!File.Exists(rxProjFile)) {
                // No rxproj file
                VsAppShell.Current.ShowMessage(Resources.OpenRProjMessage, MessageButtons.OK);
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Check if project is already opened. If so, open rproj as a text file
            var solution = VsAppShell.Current.GetGlobalService<IVsSolution>(typeof(SVsSolution));

            Guid projType;
            solution.GetProjectTypeGuid(0, rxProjFile, out projType);
            if (projType != Guid.Empty) {
                // Already opened. Open RProj as a text file
                return VSConstants.S_FALSE;
            }

            VsAppShell.Current.DispatchOnUIThread(() => {
                Guid iidProject = Guid.Empty;
                IntPtr ppProject;
                int hr = solution.CreateProject(RGuidList.CpsProjectFactoryGuid, rxProjFile,
                                                Path.GetDirectoryName(documentMoniker), Path.GetFileNameWithoutExtension(documentMoniker),
                                                (int)__VSCREATEPROJFLAGS.CPF_OPENFILE, ref iidProject, out ppProject);
            });

            // Not opening the file in the editor and instead opening the matching project
            return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
        }
    }
}
