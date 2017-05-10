// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using IVsServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.R.Package.Editors {
    using Package = VisualStudio.Shell.Package;

    /// <summary>
    /// Base editor factory for all Web editors
    /// </summary>
    public abstract class BaseEditorFactory : IVsEditorFactory, IDisposable {
        private readonly IServiceContainer _services;
        private readonly IVsEditorAdaptersFactoryService _adaptersFactory;
        private readonly Guid _editorFactoryId;
        private bool _openWithEncoding;

        protected Microsoft.VisualStudio.Shell.Package Package { get; private set; }
        protected IVsServiceProvider VsServiceProvider { get; private set; }
        protected List<TextBufferInitializationTracker> InitializationTrackers { get; }
        protected Guid LanguageServiceId { get; }

        public BaseEditorFactory(Package package, IServiceContainer services, Guid editorFactoryId, Guid languageServiceId, bool openWithEncoding = false) {
            _services = services;
            _adaptersFactory = services.GetService<IVsEditorAdaptersFactoryService>();
            Package = package;
            InitializationTrackers = new List<TextBufferInitializationTracker>();
            _editorFactoryId = editorFactoryId;
            _openWithEncoding = openWithEncoding;
            LanguageServiceId = languageServiceId;
        }

        public void SetEncoding(bool value) => _openWithEncoding = value;

        public virtual int CreateEditorInstance(
           uint createEditorFlags,
           string documentMoniker,
           string physicalView,
           IVsHierarchy hierarchy,
           uint itemid,
           IntPtr docDataExisting,
           out IntPtr docView,
           out IntPtr docData,
           out string editorCaption,
           out Guid commandUIGuid,
           out int createDocumentWindowFlags) {

            return CreateEditorInstance(createEditorFlags, documentMoniker, physicalView, hierarchy, itemid, docDataExisting,
                                        LanguageServiceId, out docView, out docData, out editorCaption, out commandUIGuid, out createDocumentWindowFlags);
        }

        protected int CreateEditorInstance(
           uint createEditorFlags,
           string documentMoniker,
           string physicalView,
           IVsHierarchy hierarchy,
           uint itemid,
           IntPtr docDataExisting,
           Guid languageServiceId,
           out IntPtr docView,
           out IntPtr docData,
           out string editorCaption,
           out Guid commandUIGuid,
           out int createDocumentWindowFlags) {
            // Initialize output parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = _editorFactoryId;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0) {
                return VSConstants.E_INVALIDARG;
            }

            // Get a text buffer
            var textLines = GetTextBuffer(docDataExisting, languageServiceId);
            if (IsIncompatibleContentType(textLines)) {
                // Unknown docData type then, so we have to force VS to close the other editor.
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            if (_openWithEncoding) {
                // Force to close the editor if it's currently open
                if (docDataExisting != IntPtr.Zero) {
                    docView = IntPtr.Zero;
                    docData = IntPtr.Zero;
                    commandUIGuid = Guid.Empty;
                    createDocumentWindowFlags = 0;
                    return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
                }

                var userData = textLines as IVsUserData;
                if (userData != null) {
                    int hresult = userData.SetData(
                        VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid,
                        (uint)__PROMPTONLOADFLAGS.codepagePrompt);

                    if (ErrorHandler.Failed(hresult)) {
                        return hresult;
                    }
                }
            }

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero) {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            } else {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try {
                docView = CreateDocumentView(
                    physicalView,
                    documentMoniker,
                    hierarchy,
                    (VSConstants.VSITEMID)itemid,
                    textLines,
                    docDataExisting,
                    languageServiceId,
                    out editorCaption);
            } finally {
                if (docView == IntPtr.Zero) {
                    if (docDataExisting != docData && docData != IntPtr.Zero) {
                        // Cleanup the instance of the docData that we have addref'ed
                        Marshal.Release(docData);
                        docData = IntPtr.Zero;
                    }
                }
            }

            return VSConstants.S_OK;
        }

        protected virtual bool IsIncompatibleContentType(IVsTextLines textLines) {
            return false;
        }

        private IVsTextLines GetTextBuffer(IntPtr docDataExisting, Guid languageServiceId) {
            IVsTextLines textLines;

            if (docDataExisting == IntPtr.Zero) {
                // Create a new IVsTextLines buffer.
                var clsid = typeof(VsTextBufferClass).GUID;
                var riid = typeof(IVsTextLines).GUID;
                textLines = Package.CreateInstance(ref clsid, ref riid, typeof(IVsTextLines)) as IVsTextLines;
                Debug.Assert(textLines != null);

                // set the buffer's site
                ((IObjectWithSite)textLines).SetSite(VsServiceProvider);
                textLines.SetLanguageServiceID(ref languageServiceId);
            } else {
                // Use the existing text buffer
                var dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;

                if (textLines == null) {
                    // Try get the text buffer from textbuffer provider
                    var textBufferProvider = dataObject as IVsTextBufferProvider;
                    textBufferProvider?.GetTextBuffer(out textLines);
                }
            }

            if (textLines == null) {
                // Unknown docData type then, so we have to force VS to close the other editor.
                ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_INCOMPATIBLEDOCDATA);
            }

            return textLines;
        }

        private IntPtr CreateDocumentView(
            string physicalView,
            string documentName,
            IVsHierarchy hierarchy,
            VSConstants.VSITEMID itemid,
            IVsTextLines textLines,
            IntPtr docDataExisting,
            Guid languageServiceId,
            out string editorCaption) {
            // Init out params
            editorCaption = string.Empty;

            if (string.IsNullOrEmpty(physicalView)) {
                // create code window as default physical view
                return CreateTextView(textLines, docDataExisting, languageServiceId, out editorCaption);
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_UNSUPPORTEDFORMAT);
            return IntPtr.Zero;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private IntPtr CreateTextView(
            IVsTextLines textLines,
            IntPtr docDataExisting,
            Guid languageServiceId,
            out string editorCaption) {
            IVsCodeWindow window = _adaptersFactory.CreateVsCodeWindowAdapter(VsServiceProvider);
            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            CreateTextBufferInitializationTracker(textLines, docDataExisting, languageServiceId);
            return Marshal.GetIUnknownForObject(window);
        }

        protected virtual void CreateTextBufferInitializationTracker(
            IVsTextLines textLines,
            IntPtr docDataExisting,
            Guid languageServiceId) {
            // At this point buffer hasn't been initialized with content yet and hence we cannot 
            // get ITextBuffer from the adapter. In order to get text buffer and properly attach 
            // view filters we need to create a proxy class which will get called when document 
            // is fully loaded and text buffer is finally populated with content.

            var tracker = new TextBufferInitializationTracker(_services, textLines, languageServiceId, InitializationTrackers);
            if (docDataExisting != IntPtr.Zero) {
                // When creating a new view for an existing buffer, the tracker object has to clean itself up
                tracker.OnLoadCompleted(0);
            }
        }

        public virtual int SetSite(IVsServiceProvider psp) {
            VsServiceProvider = psp;
            return VSConstants.S_OK;
        }

        public virtual int Close() {
            VsServiceProvider = null;
            Package = null;
            return VSConstants.S_OK;
        }

        public int MapLogicalView(ref Guid logicalView, out string physicalView) {
            // initialize out parameter
            physicalView = null;

            // Determine the physical view
            // {alexgav} LOGVIEWID_Code is needed by JavaScript Language Service
            // See bug 663657 Double clicking on error list error will try to use legacy editor 
            // to open the file (instead of staying in the libra editor)
            if (VSConstants.LOGVIEWID_Primary == logicalView ||
                VSConstants.LOGVIEWID_TextView == logicalView ||
                VSConstants.LOGVIEWID_Code == logicalView ||
                VSConstants.LOGVIEWID_Debugging == logicalView) {
                return VSConstants.S_OK;
            }

            // E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
            return VSConstants.E_NOTIMPL;
        }

        #region IDisposable
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
        #endregion

        public static void InitKeyBindings(ITextView textView) {
            var vsTextView = textView.GetViewAdapter<IVsTextView>();
            var os = vsTextView as IObjectWithSite;
            var unkSite = IntPtr.Zero;
            var unkFrame = IntPtr.Zero;

            try {
                os.GetSite(typeof(IVsServiceProvider).GUID, out unkSite);
                var sp = Marshal.GetObjectForIUnknown(unkSite) as OLE.Interop.IServiceProvider;

                sp.QueryService(typeof(SVsWindowFrame).GUID, typeof(IVsWindowFrame).GUID, out unkFrame);
                var frame = Marshal.GetObjectForIUnknown(unkFrame) as IVsWindowFrame;
                frame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, VSConstants.GUID_TextEditorFactory);
            } finally {
                if (unkSite != IntPtr.Zero) {
                    Marshal.Release(unkSite);
                }
                if (unkFrame != IntPtr.Zero) {
                    Marshal.Release(unkFrame);
                }
            }
        }

    }
}
