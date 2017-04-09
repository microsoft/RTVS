// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Projection;
using Microsoft.R.Components.Extensions;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class ViewUtilities {
        private static IVsEditorAdaptersFactoryService _adaptersFactoryService;
        public static IVsEditorAdaptersFactoryService AdaptersFactoryService {
            get {
                _adaptersFactoryService = _adaptersFactoryService ?? VsAppShell.Current.GetService<IVsEditorAdaptersFactoryService>();
                return _adaptersFactoryService;
            }
            internal set {
                _adaptersFactoryService = value;
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

        public static T GetService<T>(this ITextView textView, Type type = null) where T : class {
            var vsTextView = AdaptersFactoryService.GetViewAdapter(textView);
            return vsTextView?.GetService<T>(type);
        }

        public static T GetService<T>(this IVsTextView vsTextView, Type type = null) where T : class {
            var ows = vsTextView as IObjectWithSite;
            type = type ?? typeof(T);

            IntPtr sitePtr;
            var serviceProviderGuid = typeof(OLE.Interop.IServiceProvider).GUID;

            ows.GetSite(ref serviceProviderGuid, out sitePtr);

            var oleSP = Marshal.GetObjectForIUnknown(sitePtr) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
            Marshal.Release(sitePtr);

            var sp = new ServiceProvider(oleSP);
            return sp.GetService(type) as T;
        }

        public static T GetViewAdapter<T>(this ITextView textView) where T : class, IVsTextView {
            var vsTextView = AdaptersFactoryService.GetViewAdapter(textView);
            return vsTextView as T;
        }

        public static bool GetIUnknownProperty(this IVsWindowFrame windowFrame, __VSFPROPID propid, out object result) {
            result = null;
            windowFrame?.GetProperty((int)propid, out result);
            return result != null;
        }

        public static string GetFilePath(this ITextView textView) {
            string path = null;
            if (textView != null && !textView.IsClosed) {
                if (textView.TextBuffer is IProjectionBuffer) {
                    var pbm = ProjectionBufferManager.FromTextBuffer(textView.TextBuffer);
                    path = pbm?.DiskBuffer.GetFilePath();
                }
                if (string.IsNullOrEmpty(path)) {
                    path = textView.TextBuffer.GetFilePath();
                }
            }
            return path;
        }

        /// <summary>
        /// Converts Span to [legacy] TextSpan structure that is used in IVs* interfaces
        /// </summary>
        public static TextSpan ToTextSpan(this Span span, ITextBuffer textBuffer) {
            var ts = new TextSpan();
            var startLine = textBuffer.CurrentSnapshot.GetLineFromPosition(span.Start);
            var endLine = textBuffer.CurrentSnapshot.GetLineFromPosition(span.End);
            ts.iStartLine = startLine.LineNumber;
            ts.iEndLine = endLine.LineNumber;
            ts.iStartIndex = span.Start - startLine.Start;
            ts.iEndIndex = span.End - endLine.Start;
            return ts;
        }
    }
}
