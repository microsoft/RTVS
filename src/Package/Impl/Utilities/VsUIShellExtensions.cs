// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    internal static class VsUIShellExtensions {
        public static IEnumerable<IVsWindowFrame> EnumerateWindows(this IVsUIShell4 shell, __WindowFrameTypeFlags flags, Guid? windowGuid = null) {
            Dispatcher.CurrentDispatcher.VerifyAccess();

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
    }
}
