// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IFileEditor))]
    internal sealed class FileEditor : IFileEditor, IVsWindowFrameEvents {
        private readonly IApplicationShell _appShell;
        private readonly IVsEditorAdaptersFactoryService _adapterService;
        private IVsWindowFrame _editorFrame;
        private ITextBuffer _textBuffer;
        private IVsUIShell7 _uiShell;
        private TaskCompletionSource<string> _tcs;
        private uint _cookie;

        [ImportingConstructor]
        public FileEditor(IApplicationShell appShell, IVsEditorAdaptersFactoryService adapterService) {
            _appShell = appShell;
            _adapterService = adapterService;
            _appShell.Terminating += OnAppTerminating;
        }

        private void OnAppTerminating(object sender, EventArgs e) {
            if (_cookie != 0) {
                _uiShell?.UnadviseWindowFrameEvents(_cookie);
                _cookie = 0;
                _tcs?.SetCanceled();
            }
        }

        public Task<string> EditFileAsync(string content, string fileName, CancellationToken cancellationToken = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();
            return Task.Run(async () => {
                _tcs = new TaskCompletionSource<string>();

                if (!string.IsNullOrEmpty(content)) {
                    var formatter = new RFormatter(REditorSettings.FormatOptions);
                    content = formatter.Format(content);

                    var fs = _appShell.Services.FileSystem;
                    fileName = Path.ChangeExtension(Path.GetTempFileName(), ".r");
                    try {
                        if (fs.FileExists(fileName)) {
                            fs.DeleteFile(fileName);
                        }
                        using (var sw = new StreamWriter(fileName)) {
                            sw.Write(content);
                        }
                    } catch (IOException) { } catch (UnauthorizedAccessException) { }
                } else {
                    fileName = fileName?.FromRPath();
                }

                if(!string.IsNullOrEmpty(fileName)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _appShell.SwitchToMainThreadAsync(cancellationToken);

                    IVsUIHierarchy hier;
                    uint itemid;
                    IVsTextView view;

                    VsShellUtilities.OpenDocument(RPackage.Current, fileName, VSConstants.LOGVIEWID.Code_guid, out hier, out itemid, out _editorFrame, out view);
                    if (view == null || _editorFrame == null) {
                        return string.Empty;
                    }
                    cancellationToken.ThrowIfCancellationRequested();

                    IVsTextLines vsTextLines;
                    view.GetBuffer(out vsTextLines);
                    _textBuffer = _adapterService.GetDataBuffer(vsTextLines as IVsTextBuffer);

                    _uiShell = _appShell.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
                    _cookie = _uiShell.AdviseWindowFrameEvents(this);

                    await TaskUtilities.SwitchToBackgroundThread();
                    cancellationToken.ThrowIfCancellationRequested();

                    return await _tcs.Task;
                }

                return string.Empty;
            }, cancellationToken);
        }

        #region IVsWindowFrameEvents
        public void OnFrameDestroyed(IVsWindowFrame frame) {
            if (frame == _editorFrame) {
                _uiShell.UnadviseWindowFrameEvents(_cookie);
                _cookie = 0;
                _tcs?.SetResult(_textBuffer.CurrentSnapshot.GetText());
            }
        }

        public void OnFrameCreated(IVsWindowFrame frame) { }
        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible) { }
        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen) { }
        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame) { }
        #endregion
    }
}
