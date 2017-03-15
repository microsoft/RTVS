// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Document;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Editors {
    public sealed class TextBufferInitializationTracker : IVsTextBufferDataEvents {
        private IVsTextLines _textLines;
        private uint cookie = 0;
        private IConnectionPoint cp = null;
        private List<TextBufferInitializationTracker> _trackers;
        private string _documentName;
        private IVsHierarchy _hierarchy;
        private VSConstants.VSITEMID _itemid;
        private Guid _languageServiceGuid;

        #region Constructors
        public TextBufferInitializationTracker(
            string documentName,
            IVsHierarchy hierarchy,
            VSConstants.VSITEMID itemid,
            IVsTextLines textLines,
            Guid languageServiceId,
            List<TextBufferInitializationTracker> trackers) {
            Vsshell.Current.CompositionService.SatisfyImportsOnce(this);

            _documentName = documentName;
            _hierarchy = hierarchy;
            _itemid = itemid;
            _textLines = textLines;
            _languageServiceGuid = languageServiceId;
            _trackers = trackers;

            IConnectionPointContainer cpc = textLines as IConnectionPointContainer;
            Guid g = typeof(IVsTextBufferDataEvents).GUID;
            cpc.FindConnectionPoint(g, out cp);
            cp.Advise(this, out cookie);

            _trackers.Add(this);
        }
        #endregion

        #region IVsTextBufferDataEvents
        public void OnFileChanged(uint grfChange, uint dwFileAttrs) {
        }

        public int OnLoadCompleted(int fReload) {
            var adapterService = Vsshell.Current.ExportProvider.GetExport<IVsEditorAdaptersFactoryService>().Value;

            // Set language service ID as early as possible, since it may change content type of the buffer,
            // e.g. in a weird scenario when someone does "Open With X Editor" on an Y file. Calling this
            // will change content type to the one language service specifies instead of the default one for
            // the file extension, and will ensure that correct editor factory is used.
            _textLines.SetLanguageServiceID(ref _languageServiceGuid);
            ITextBuffer diskBuffer = adapterService.GetDocumentBuffer(_textLines);
            Debug.Assert(diskBuffer != null);

            try {
                var editorInstance = ServiceManager.GetService<IEditorInstance>(diskBuffer);
                if (editorInstance == null) {
                    var importComposer = new ContentTypeImportComposer<IEditorFactory>(Vsshell.Current.CompositionService);
                    var instancefactory = importComposer.GetImport(diskBuffer.ContentType.TypeName);
                    Debug.Assert(instancefactory != null);

                    var documentFactoryImportComposer = new ContentTypeImportComposer<IVsEditorDocumentFactory>(Vsshell.Current.CompositionService);
                    var documentFactory = documentFactoryImportComposer.GetImport(diskBuffer.ContentType.TypeName);
                    Debug.Assert(documentFactory != null);

                    editorInstance = instancefactory.CreateEditorInstance(diskBuffer, documentFactory);
                }

                Debug.Assert(editorInstance != null);
                adapterService.SetDataBuffer(_textLines, editorInstance.ViewBuffer);
            } finally {
                cp.Unadvise(cookie);
                cookie = 0;

                _textLines = null;
                _hierarchy = null;

                _trackers.Remove(this);
                _trackers = null;
            }

            return VSConstants.S_OK;
        }
        #endregion
    }
}
