using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class ViewUtilities {
        private static IVsEditorAdaptersFactoryService _adaptersFactoryService;
        private static IVsEditorAdaptersFactoryService AdaptersFactoryService {
            get {
                if (_adaptersFactoryService == null)
                    _adaptersFactoryService = EditorShell.Current.ExportProvider.GetExport<IVsEditorAdaptersFactoryService>().Value;

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

        public static int ViewColumnFromString(IContentType contentType, string str) {
            if (String.IsNullOrEmpty(str))
                return 0;

            var settings = EditorShell.GetSettings(contentType.DisplayName);
            var tabSize = settings.GetInteger(CommonSettings.FormatterTabSizeKey, 4);

            return TextHelper.ConvertTabsToSpaces(str, tabSize).Length;
        }

        public static ITextView ActiveTextView {
            get {
                IVsTextView vsTextView = null;
                ITextView activeTextView = null;

                IVsTextManager2 textManager = AppShell.Current.GetGlobalService<IVsTextManager2>(typeof(SVsTextManager));

                if (ErrorHandler.Succeeded(textManager.GetActiveView2(0, null, (uint)(_VIEWFRAMETYPE.vftCodeWindow | _VIEWFRAMETYPE.vftToolWindow), out vsTextView))) {
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
    }
}
