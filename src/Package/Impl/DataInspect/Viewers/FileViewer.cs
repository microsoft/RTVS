// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    internal static class FileViewer {
        public static void ViewFile(string fileName, string caption) {
            IVsUIHierarchy hier;
            UInt32 itemid;
            IVsWindowFrame frame;
            IVsTextView view;
            VsShellUtilities.OpenDocument(RPackage.Current, fileName.FromRPath(), VSConstants.LOGVIEWID.Code_guid, out hier, out itemid, out frame, out view);

            IVsTextLines textBuffer = null;
            frame?.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, caption);
            view?.GetBuffer(out textBuffer);
            textBuffer?.SetStateFlags((uint)BUFFERSTATEFLAGS.BSF_USER_READONLY);
        }
    }
}
