// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export]
    [Export(typeof(IActiveRInteractiveWindowTracker))]
    internal class VsActiveRInteractiveWindowTracker : IActiveRInteractiveWindowTracker, IVsWindowFrameEvents {
        private IInteractiveWindowVisualComponent _lastActiveWindow;

        public IInteractiveWindowVisualComponent LastActiveWindow => _lastActiveWindow;
        public event EventHandler<InteractiveWindowChangedEventArgs> LastActiveWindowChanged;

        public void OnFrameCreated(IVsWindowFrame frame) {
        }

        public void OnFrameDestroyed(IVsWindowFrame frame) {
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible) {
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen) {
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame) {
            var interactiveWindow = GetComponent(oldFrame);
            if (interactiveWindow != null) {
                UpdateInteractiveWindowIfRequired(interactiveWindow);
            }

            interactiveWindow = GetComponent(newFrame);
            if (interactiveWindow != null) {
                UpdateInteractiveWindowIfRequired(interactiveWindow);
            }
        }

        private void UpdateInteractiveWindowIfRequired(IInteractiveWindowVisualComponent newInteractiveWindow) {
            var oldInteractiveWindow = Interlocked.Exchange(ref _lastActiveWindow, newInteractiveWindow);
            if (oldInteractiveWindow == newInteractiveWindow) {
                return;
            }

            var handler = LastActiveWindowChanged;
            handler?.Invoke(this, new InteractiveWindowChangedEventArgs(oldInteractiveWindow, newInteractiveWindow));
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(1);
        }

        private IInteractiveWindowVisualComponent GetComponent(IVsWindowFrame frame) {
            if (frame == null) {
                return null;
            }

            Guid property;
            if (ErrorHandler.Failed(frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out property)) || property != RGuidList.ReplInteractiveWindowProviderGuid) {
                return null;
            }

            object docView;
            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView))) {
                return null;
            }

            var interactiveWindow = (docView as IVsInteractiveWindow)?.InteractiveWindow;
            if (interactiveWindow == null) {
                return null;
            }

            IInteractiveWindowVisualComponent component;
            return interactiveWindow.Properties.TryGetProperty(typeof(IInteractiveWindowVisualComponent), out component) ? component : null;
        }
    }

    public class InteractiveWindowChangedEventArgs {
        public IInteractiveWindowVisualComponent Old { get; set; }
        public IInteractiveWindowVisualComponent New { get; set; }

        public InteractiveWindowChangedEventArgs(IInteractiveWindowVisualComponent oldWindow, IInteractiveWindowVisualComponent newWindow) {
            Old = oldWindow;
            New = newWindow;
        }
    }
}