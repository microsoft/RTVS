// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal class SendFileCommandBase {
        private readonly IRInteractiveWorkflowVisualProvider _interactiveWorkflowProvider;
        private readonly IFileSystem _fs;
        private readonly IUIService _ui;

        protected SendFileCommandBase(IRInteractiveWorkflowVisualProvider interactiveWorkflowProvider, IUIService ui, IFileSystem fs) {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
            _ui = ui;
            _fs = fs;
        }

        protected Task SendToRemoteAsync(IEnumerable<string> files, string projectDir, string projectName, string remotePath) {
            _ui.ProgressDialog.Show(async (p, ct) => await SendToRemoteWorkerAsync(files, projectDir, projectName, remotePath, p, ct), Resources.Info_TransferringFiles, 100, 500);
            return Task.CompletedTask;
        }

        private async Task SendToRemoteWorkerAsync(IEnumerable<string> files, string projectDir, string projectName, string remotePath, IProgress<ProgressDialogData> progress, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var workflow = _interactiveWorkflowProvider.GetOrCreate();
            IConsole console = new InteractiveWindowConsole(workflow);

            try {
                var session = workflow.RSession;
                int count = 0;
                int total = files.Count();

                progress.Report(new ProgressDialogData(0, Resources.Info_CompressingFiles));

                if (ct.IsCancellationRequested) {
                    return;
                }

                List<string> paths = new List<string>();
                // Compression phase : 1 of 3 phases.
                string compressedFilePath = string.Empty;
                compressedFilePath = _fs.CompressFiles(files, projectDir, new Progress<string>((p) => {
                    Interlocked.Increment(ref count);
                    int step = (count * 100 / total) / 3; // divide by 3, this is for compression phase.
                    progress.Report(new ProgressDialogData(step, Resources.Info_CompressingFile.FormatInvariant(Path.GetFileName(p), _fs.FileSize(p))));
                    string dest = p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName);
                    paths.Add($"{Resources.Info_LocalFilePath.FormatInvariant(p)}{Environment.NewLine}{Resources.Info_RemoteFilePath.FormatInvariant(dest)}");
                }), ct);

                if (ct.IsCancellationRequested) {
                    return;
                }

                using (var fts = new DataTransferSession(session, _fs)) {
                    long size = _fs.FileSize(compressedFilePath);

                    // Transfer phase: 2 of 3 phases
                    var remoteFile = await fts.SendFileAsync(compressedFilePath, true, new Progress<long>((b) => {
                        int step = 33; // start with 33% to indicate compression phase is done.
                        step += (int)((((double)b / (double)size) * 100) / 3); // divide by 3, this is for transfer phase.
                        progress.Report(new ProgressDialogData(step, Resources.Info_TransferringFilesWithSize.FormatInvariant(b, size)));
                    }), ct);

                    if (ct.IsCancellationRequested) {
                        return;
                    }

                    // Extract phase: 3 of 3 phases
                    // start with 66% completion to indicate compression and transfer phases are done.
                    progress.Report(new ProgressDialogData(66, Resources.Info_ExtractingFilesInRHost));
                    await session.EvaluateAsync<string>($"rtvs:::save_to_project_folder({remoteFile.Id}, {projectName.ToRStringLiteral()}, '{remotePath.ToRPath()}')", REvaluationKind.Normal, ct);

                    progress.Report(new ProgressDialogData(100, Resources.Info_TransferringFilesDone));

                    paths.ForEach((s) => console.WriteLine(s));
                }
            } catch (TaskCanceledException) {
                console.WriteErrorLine(Resources.Info_FileTransferCanceled);
            } catch (RHostDisconnectedException rhdex) {
                console.WriteErrorLine(Resources.Error_CannotTransferNoRSession.FormatInvariant(rhdex.Message));
            } catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException) {
                _ui.ShowErrorMessage(Resources.Error_CannotTransferFile.FormatInvariant(ex.Message));
            }
        }
    }
}
