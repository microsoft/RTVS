using System;
using System.Runtime.InteropServices;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Workspace;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class ViewUtilities {
        private static IVsEditorAdaptersFactoryService _adaptersFactoryService;
        private static IVsEditorAdaptersFactoryService AdaptersFactoryService {
            get {
                if (_adaptersFactoryService == null)
                    _adaptersFactoryService = VsAppShell.Current.ExportProvider.GetExport<IVsEditorAdaptersFactoryService>().Value;

                return _adaptersFactoryService;
            }
        }

        public static IVsWindowFrame GetActiveFrame() {
            var monitorSelection = ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (monitorSelection != null) {
                object value;
                monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out value);

                return value as IVsWindowFrame;
            }

            return null;
        }

        public static ITextView ActiveTextView {
            get {
                IVsTextView vsTextView = null;
                ITextView activeTextView = null;

                IVsTextManager2 textManager = VsAppShell.Current.GetGlobalService<IVsTextManager2>(typeof(SVsTextManager));

                if (ErrorHandler.Succeeded(textManager.GetActiveView2(0, null, (uint)(_VIEWFRAMETYPE.vftCodeWindow), out vsTextView))) {
                    activeTextView = AdaptersFactoryService.GetWpfTextView(vsTextView);
                }

                return activeTextView;
            }
        }

        public static T GetService<T>(this ITextView textView, Type type = null) where T : class {
            IVsTextView vsTextView = AdaptersFactoryService.GetViewAdapter(textView);

            return vsTextView != null ? vsTextView.GetService<T>(type) : null;
        }

        public static T GetService<T>(this IVsTextView vsTextView, Type type = null) where T : class {
            var ows = vsTextView as IObjectWithSite;

            if (type == null)
                type = typeof(T);

            IntPtr sitePtr;
            Guid serviceProviderGuid = typeof(OLE.Interop.IServiceProvider).GUID;

            ows.GetSite(ref serviceProviderGuid, out sitePtr);

            var oleSP = Marshal.GetObjectForIUnknown(sitePtr) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
            Marshal.Release(sitePtr);

            ServiceProvider sp = new ServiceProvider(oleSP);
            return sp.GetService(type) as T;
        }

        public static T QueryInterface<T>(this ITextView textView) where T : class {
            var vsTextView = AdaptersFactoryService.GetViewAdapter(textView);

            return vsTextView as T;
        }

        public static bool GetIUnknownProperty(this IVsWindowFrame windowFrame, __VSFPROPID propid, out object result) {
            result = null;

            if (windowFrame != null)
                windowFrame.GetProperty((int)propid, out result);

            return result != null;
        }

        public static void SaveFile(this ITextView textView) {
            RunningDocumentTable rdt = new RunningDocumentTable(RPackage.Current);
            string filePath = VsFileInfo.GetFileName(textView);
            rdt.SaveFileIfDirty(filePath);
        }

        public static void SourceActiveFile() {
            var activeView = ViewUtilities.ActiveTextView;
            if (activeView != null) {
                var controller = RMainController.FromTextView(activeView);
                if (controller != null) {
                    object o = null;
                    controller.Invoke(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSourceRScript, null, ref o);
                }
            }
        }
    }
}
