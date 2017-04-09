// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    internal static class VsUIShellExtensions {
        public static IEnumerable<IVsWindowFrame> EnumerateWindows(this IVsUIShell4 shell, __WindowFrameTypeFlags flags, Guid? windowGuid = null) {
            IEnumWindowFrames enumerator;
            ErrorHandler.ThrowOnFailure(shell.GetWindowEnum((uint)flags, out enumerator));

            var frames = new IVsWindowFrame[1];
            uint fetched = 0;
            while (VSConstants.S_OK == enumerator.Next(1, frames, out fetched) && fetched > 0) {
                var frame = frames[0];

                bool include = true;
                if (windowGuid.HasValue) {
                    Guid persist;
                    ErrorHandler.ThrowOnFailure(frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out persist));
                    include = persist == windowGuid;
                }

                if (include) {
                    yield return frame;
                }
            }
        }

        public static IVsWindowFrame FindDocumentFrame(this IVsUIShell4 shell, string filePath) {
            return shell.EnumerateWindows(__WindowFrameTypeFlags.WINDOWFRAMETYPE_Document).Where(f => {
                object docPath;
                f.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out docPath);
                return string.Compare(docPath as string, filePath, StringComparison.OrdinalIgnoreCase) == 0;
            }).FirstOrDefault();
        }

        public static bool TryGetUiHierarchy(this IVsUIShellOpenDocument shellDoc, string filePath, out IVsUIHierarchy uiHier, out uint vsItemId, out OLE.Interop.IServiceProvider sp) {
            int docInProject;
            var hr = shellDoc.IsDocumentInAProject(filePath, out uiHier, out vsItemId, out sp, out docInProject);
            return ErrorHandler.Succeeded(hr) && uiHier != null;
        }

        public static bool OpenFile(this IVsUIShellOpenDocument shellDoc, string filePath, out IVsWindowFrame vsWindowFrame) {
            vsWindowFrame = null;

            var logView = VSConstants.LOGVIEWID.TextView_guid;
            var caption = Path.GetFileName(filePath);
            IVsUIHierarchy uiHier;
            uint itemid;
            OLE.Interop.IServiceProvider sp;

            if (shellDoc.TryGetUiHierarchy(filePath, out uiHier, out itemid, out sp)) {
                shellDoc.OpenStandardEditor((uint)__VSOSEFLAGS.OSE_ChooseBestStdEditor, filePath, ref logView, caption, uiHier, itemid, IntPtr.Zero, sp, out vsWindowFrame);
            } else {
                IVsEditorFactory ef;
                Guid editorType = Guid.Empty;
                string physicalView;

                shellDoc.GetStandardEditorFactory(0, ref editorType, filePath, ref logView, out physicalView, out ef);
                if (ef != null) {
                    VsShellUtilities.OpenDocumentWithSpecificEditor(ServiceProvider.GlobalProvider, filePath, editorType,
                       VSConstants.LOGVIEWID.TextView_guid, out uiHier, out itemid, out vsWindowFrame);
                }
            }
            vsWindowFrame?.Show();
            return vsWindowFrame != null;
        }
    }
}
