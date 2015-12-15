using System;
using System.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Plots;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RHostClientApp: IRHostClientApp {
        /// <summary>
        /// Displays error message in the host-specific UI
        /// </summary>
        public async Task ShowErrorMessage(string message) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsAppShell.Current.ShowErrorMessage(message);
        }

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public async System.Threading.Tasks.Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return VsAppShell.Current.ShowMessage(message, buttons);
        }

        /// <summary>
        /// Displays R help URL in a browser on in the host app-provided window
        /// </summary>
        public async Task ShowHelp(string url) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            HelpWindowPane.Navigate(url);
        }

        /// <summary>
        /// Displays R plot in the host app-provided window
        /// </summary>
        public async Task Plot(string filePath, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);

            var frame = FindPlotWindow(__VSFINDTOOLWIN.FTW_fFindFirst | __VSFINDTOOLWIN.FTW_fForceCreate);  // TODO: acquire plot content provider through service
            if (frame != null) {
                object docView;
                ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView));
                if (docView != null) {
                    PlotWindowPane pane = (PlotWindowPane)docView;
                    pane.PlotContentProvider.LoadFile(filePath);

                    frame.ShowNoActivate();
                }
            }
        }

        /// <summary>
        /// Given CRAN mirror name returns URL
        /// </summary>
        public string CranUrlFromName(string mirrorName) {
            return CranMirrorList.UrlFromName(mirrorName);
        }

        private static IVsWindowFrame FindPlotWindow(__VSFINDTOOLWIN flags) {
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

            // First just find. If it exists, use it. 
            IVsWindowFrame frame;
            Guid persistenceSlot = typeof(PlotWindowPane).GUID;
            shell.FindToolWindow((uint)flags, ref persistenceSlot, out frame);
            return frame;
        }
    }
}
