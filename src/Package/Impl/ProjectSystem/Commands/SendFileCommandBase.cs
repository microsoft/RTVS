// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal class SendFileCommandBase {
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;
        private readonly ICoreShell _coreShell;
        private readonly IFileSystem _fs;
        private readonly IApplicationShell _appShell;

        protected SendFileCommandBase(IRInteractiveWorkflowProvider interactiveWorkflowProvider, ICoreShell coreShell, IApplicationShell appShell, IFileSystem fs) {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
            _coreShell = coreShell;
            _appShell = appShell;
            _fs = fs;
        }

        protected async Task<bool> SendToRemoteAsync(IEnumerable<string> files, string projectDir, string projectName, string remotePath) {
            IVsStatusbar statusBar = _appShell.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
            string currentStatusText;
            statusBar.GetText(out currentStatusText);

            await TaskUtilities.SwitchToBackgroundThread();
            var workflow = _interactiveWorkflowProvider.GetOrCreate();
            var outputWindow = workflow.ActiveWindow.InteractiveWindow;
            uint cookie = 0;
            try {
                var session = workflow.RSession;
                statusBar.SetText(Resources.Info_CompressingFiles);
                statusBar.Progress(ref cookie, 1, "", 0, 0);

                int count = 0;
                uint total = (uint)files.Count() * 2; // for compressing and sending
                string compressedFilePath = string.Empty;
                await Task.Run(() => {
                    compressedFilePath = _fs.CompressFiles(files, projectDir, new Progress<string>((p) => {
                        Interlocked.Increment(ref count);
                        statusBar.Progress(ref cookie, 1, string.Format(Resources.Info_CompressingFile, Path.GetFileName(p)), (uint)count, total);
                        string dest = p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName);
                        _coreShell.DispatchOnUIThread(() => {
                            outputWindow.WriteLine(string.Format(Resources.Info_LocalFilePath, p));
                            outputWindow.WriteLine(string.Format(Resources.Info_RemoteFilePath, dest));
                        });
                    }), CancellationToken.None);
                    statusBar.Progress(ref cookie, 0, "", 0, 0);
                });

                using (var fts = new DataTransferSession(session, _fs)) {
                    cookie = 0;
                    statusBar.SetText(Resources.Info_TransferringFiles);

                    total = (uint)_fs.FileSize(compressedFilePath);

                    var remoteFile = await fts.SendFileAsync(compressedFilePath, true, new Progress<long>((b) => {
                        statusBar.Progress(ref cookie, 1, Resources.Info_TransferringFiles, (uint)b, total);
                    }));

                    statusBar.SetText(Resources.Info_ExtractingFilesInRHost);
                    await session.EvaluateAsync<string>($"rtvs:::save_to_project_folder({remoteFile.Id}, {projectName.ToRStringLiteral()}, '{remotePath.ToRPath()}')", REvaluationKind.Normal);

                    _coreShell.DispatchOnUIThread(() => {
                        outputWindow.WriteLine(Resources.Info_TransferringFilesDone);
                    });
                }
            } catch (IOException ex) {
                _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotTransferFile, ex.Message));
            } catch (RHostDisconnectedException) {
                _coreShell.DispatchOnUIThread(() => {
                    outputWindow.WriteErrorLine(Resources.Error_CannotTransferNoRSession);
                });
            } finally {
                statusBar.Progress(ref cookie, 0, "", 0, 0);
                statusBar.SetText(currentStatusText);
                await _coreShell.SwitchToMainThreadAsync();
            }
            
            return true;
        }
    }
}
