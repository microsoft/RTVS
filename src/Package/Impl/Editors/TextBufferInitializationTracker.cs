// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Editors {
    public sealed class TextBufferInitializationTracker : IVsTextBufferDataEvents {
        private readonly IServiceContainer _services;
        private readonly List<TextBufferInitializationTracker> _trackers;

        private readonly IConnectionPoint cp;
        private Guid _languageServiceGuid;
        private IVsTextLines _textLines;
        private uint cookie;

        #region Constructors
        public TextBufferInitializationTracker(IServiceContainer services, IVsTextLines textLines, Guid languageServiceId, List<TextBufferInitializationTracker> trackers) {
            _services = services;
            _textLines = textLines;
            _languageServiceGuid = languageServiceId;
            _trackers = trackers;

            var cpc = textLines as IConnectionPointContainer;
            var g = typeof(IVsTextBufferDataEvents).GUID;
            cpc.FindConnectionPoint(g, out cp);
            cp.Advise(this, out cookie);

            _trackers.Add(this);
        }
        #endregion

        #region IVsTextBufferDataEvents
        public void OnFileChanged(uint grfChange, uint dwFileAttrs) { }

        public int OnLoadCompleted(int fReload) {
            // Set language service ID as early as possible, since it may change content type of the buffer,
            // e.g. in a weird scenario when someone does "Open With X Editor" on an Y file. Calling this
            // will change content type to the one language service specifies instead of the default one for
            // the file extension, and will ensure that correct editor factory is used.
            _textLines.SetLanguageServiceID(ref _languageServiceGuid);
            var adapterService = _services.GetService<IVsEditorAdaptersFactoryService>();
            var diskBuffer = adapterService.GetDocumentBuffer(_textLines);
            Debug.Assert(diskBuffer != null);

            try {
                var editorInstance = diskBuffer.GetService<IEditorViewModel>();
                if (editorInstance == null) {
                    var locator = _services.GetService<IContentTypeServiceLocator>();
                    var instancefactory = locator.GetService<IEditorViewModelFactory>(diskBuffer.ContentType.TypeName);

                    Debug.Assert(instancefactory != null, "No editor factory found for the provided text buffer");
                    editorInstance = instancefactory.CreateEditorViewModel(diskBuffer);
                }

                Debug.Assert(editorInstance != null);
                adapterService.SetDataBuffer(_textLines, editorInstance.ViewBuffer.As<ITextBuffer>());
            } finally {
                cp.Unadvise(cookie);
                cookie = 0;
                _textLines = null;
                _trackers.Remove(this);
            }
            return VSConstants.S_OK;
        }
        #endregion
    }
}
