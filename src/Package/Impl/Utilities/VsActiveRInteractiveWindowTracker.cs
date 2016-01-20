using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IActiveRInteractiveWindowTracker))]
    internal class VsActiveRInteractiveWindowTracker : IActiveRInteractiveWindowTracker, IVsWindowFrameEvents {
        private IInteractiveWindow _lastActiveWindow;

        public IInteractiveWindow LastActiveWindow => _lastActiveWindow;
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
            var interactiveWindow = GetInteractiveWindow(oldFrame);
            if (interactiveWindow != null) {
                UpdateInteractiveWindowIfRequired(interactiveWindow);
            }

            interactiveWindow = GetInteractiveWindow(newFrame);
            if (interactiveWindow != null) {
                UpdateInteractiveWindowIfRequired(interactiveWindow);
            }
        }

        private void UpdateInteractiveWindowIfRequired(IInteractiveWindow newInteractiveWindow) {
            var oldInteractiveWindow = Interlocked.Exchange(ref _lastActiveWindow, newInteractiveWindow);
            if (oldInteractiveWindow == newInteractiveWindow) {
                return;
            }

            var handler = LastActiveWindowChanged;
            handler?.Invoke(this, new InteractiveWindowChangedEventArgs(oldInteractiveWindow, newInteractiveWindow));
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(1);
        }

        private IInteractiveWindow GetInteractiveWindow(IVsWindowFrame frame) {
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

            return (docView as IVsInteractiveWindow)?.InteractiveWindow;
        }
    }

    public class InteractiveWindowChangedEventArgs {
        public IInteractiveWindow Old { get; set; }
        public IInteractiveWindow New { get; set; }

        public InteractiveWindowChangedEventArgs(IInteractiveWindow oldWindow, IInteractiveWindow newWindow) {
            Old = oldWindow;
            New = newWindow;
        }
    }
}