using System;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl {
    /// <summary>
    /// Tracks most recently active REPL window
    /// </summary>
    internal static class ReplWindow {
        public static bool IsActive {
            get {
                IVsWindowFrame frame1;
                IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;

                // First just find. If it exists, use it. 
                shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fFindFirst, ref persistenceSlot, out frame1);
                IVsWindowFrame frame = frame1;
                if (frame != null) {
                    int onScreen;
                    frame.IsOnScreen(out onScreen);
                    return onScreen != 0;
                }
                return false;
            }
        }

        public static bool ReplWindowExists() {
            IVsWindowFrame frame = FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            return frame != null;
        }

        public static void Show() {
            IVsWindowFrame frame = FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            frame?.Show();
        }

        public static async Task EnsureReplWindow() {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!ReplWindowExists()) {
                IVsWindowFrame frame = FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fForceCreate);
                if (frame != null) {
                    //IntPtr bitmap = Resources.ReplWindowIcon.GetHbitmap();
                    frame.SetProperty((int)__VSFPROPID4.VSFPROPID_TabImage, Resources.ReplWindowIcon);
                    frame.Show();
                }
            }
        }

        public static IVsWindowFrame FindReplWindowFrame(__VSFINDTOOLWIN flags) {
            IVsWindowFrame frame;
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

            Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;

            // First just find. If it exists, use it. 
            shell.FindToolWindow((uint)flags, ref persistenceSlot, out frame);
            return frame;
        }
    }
}
