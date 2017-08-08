// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using static System.FormattableString;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Host.Client {
    public sealed class DataTransferSession : IDisposable {
        private readonly IRSession _session;
        private readonly IRBlobService _blobService;
        private readonly IFileSystem _fs;
        private readonly List<IRBlobInfo> _cleanup;

        public DataTransferSession(IRSession session, IFileSystem fs) {
            _session = session;
            _blobService = _session;
            _fs = fs;
            _cleanup = new List<IRBlobInfo>();
        }

        /// <summary>
        /// Sends a block of data to R-Host by creating a new blob. This method adds the blob for clean 
        /// up by default.
        /// </summary>
        /// <param name="data">Block of data to be sent.</param>
        /// <param name="doCleanUp">
        /// true to add blob created upon transfer for cleanup on dispose, false to ignore it after transfer.
        /// </param>
        public async Task<IRBlobInfo> SendBytesAsync(byte[] data, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            IRBlobInfo blob = null;
            using (RBlobStream blobStream = await RBlobStream.CreateAsync(_blobService))
            using (Stream ms = new MemoryStream(data)) {
                await ms.CopyToAsync(blobStream, progress, cancellationToken);
                blob = blobStream.GetBlobInfo();
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
            return blob;
        }

        /// <summary>
        /// Sends file to R-Host by creating a new blob. This method adds the blob for clean up by default. 
        /// </summary>
        /// <param name="filePath">Path to the file to be sent to R-Host.</param>
        /// <param name="doCleanUp">
        /// true to add blob created upon transfer for cleanup on dispose, false to ignore it after transfer.
        /// </param>
        public async Task<IRBlobInfo> SendFileAsync(string filePath, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            IRBlobInfo blob = null;
            using (RBlobStream blobStream = await RBlobStream.CreateAsync(_blobService))
            using (Stream fileStream = _fs.FileOpen(filePath, FileMode.Open)){
                await fileStream.CopyToAsync(blobStream, progress, cancellationToken);
                blob = blobStream.GetBlobInfo();
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
            return blob;
        }

        /// <summary>
        /// Gets the data for a given blob from R-Host. Saves the data to <paramref name="filePath"/>. This 
        /// method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blob">Blob from which the data is to be retrieved.</param>
        /// <param name="filePath">Path to the file where the retrieved data will be written.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task FetchFileAsync(IRBlobInfo blob, string filePath, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _blobService))
            using (Stream fileStream = _fs.CreateFile(filePath)) {
                await blobStream.CopyToAsync(fileStream, progress, cancellationToken);
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
        }

        /// <summary>
        /// Gets the data for a given blob from R-Host. Decompresses it and saves the data to <paramref name="filePath"/>. This 
        /// method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blob">Blob from which the data is to be retrieved.</param>
        /// <param name="filePath">Path to the file where the retrieved data will be written.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task FetchAndDecompressFileAsync(IRBlobInfo blob, string filePath, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            using (MemoryStream compressed = new MemoryStream(await FetchBytesAsync(blob, false, progress, cancellationToken)))
            using (ZipArchive archive = new ZipArchive(compressed, ZipArchiveMode.Read)) {
                var entry = archive.GetEntry("data");
                using (Stream decompressedStream = entry.Open())
                using (Stream fileStream = _fs.CreateFile(filePath)) {
                    await decompressedStream.CopyToAsync(fileStream, progress, cancellationToken);
                }
            }
            if (doCleanUp) {
                _cleanup.Add(blob);
            }
        }

        /// <summary>
        /// Gets the data for a given blob from R-Host. Decompresses it and saves the data to <paramref name="filePath"/>. This 
        /// method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blobId">Data block on remote machine</param>
        /// <param name="filePath">Path to the file where the retrieved data will be written.</param>
        public async Task FetchAndDecompressFileAsync(ulong blobId, string filePath, 
            IProgress<ProgressDialogData> progress, string progressMessage, CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            var total = await _session.GetBlobSizeAsync(blobId, cancellationToken);
            progress.Report(new ProgressDialogData(0, statusBarText: progressMessage, waitMessage: progressMessage));
            await FetchAndDecompressFileAsync(new RBlobInfo(blobId), filePath, true, new Progress<long>(b => {
                var step = (int)(b * 100 / total);
                progress.Report(new ProgressDialogData(step, statusBarText: progressMessage, waitMessage: progressMessage));
            }), cancellationToken);
            progress.Report(new ProgressDialogData(100, statusBarText: progressMessage, waitMessage: progressMessage));
        }

        /// <summary>
        /// Gets the data for a given blob from R-Host. This method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blob">Blob from which the data is to be retrieved.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task<byte[]> FetchBytesAsync(IRBlobInfo blob, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            byte[] data = null;
            using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _blobService, cancellationToken))
            using (MemoryStream ms = new MemoryStream((int)blobStream.Length)) {
                await blobStream.CopyToAsync(ms, progress, cancellationToken);
                data = ms.ToArray();
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }

            return data;
        }

        /// <summary>
        /// Gets contents of a remote file and copies it to given stream. This method adds the blob for clean up by default.
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="stream"></param>
        /// <param name="doCleanUp"></param>
        public async Task CopyToFileStreamAsync(string remoteFile, Stream stream, bool doCleanUp = true, IProgress<long> progress = null, CancellationToken cancellationToken = default(CancellationToken)) {
            string filePath = remoteFile.ToRPath().ToRStringLiteral();
            IRBlobInfo blob = new RBlobInfo(await _session.EvaluateAsync<ulong>($"rtvs:::create_blob(readBin({filePath}, 'raw', file.info({filePath})$size))", REvaluationKind.Normal, cancellationToken));
            using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _blobService, cancellationToken)) {
                await blobStream.CopyToAsync(stream, progress, cancellationToken);
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
        }

        /// <summary>
        /// Gets the data for a given blob (compressed) from R-Host and decompresses it. This 
        /// method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blob">Blob from which the data is to be retrieved.</param>
        /// <param name="filePath">Path to the file where the retrieved data will be written.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task<byte[]> FetchAndDecompressBytesAsync(IRBlobInfo blob, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            byte[] data = null;
            using (MemoryStream compressed = new MemoryStream(await FetchBytesAsync(blob, false, progress, cancellationToken)))
            using (ZipArchive archive = new ZipArchive(compressed, ZipArchiveMode.Read)) {
                var entry = archive.GetEntry("data");
                using (Stream decompressedStream = entry.Open())
                using (MemoryStream ms = new MemoryStream()) {
                    await decompressedStream.CopyToAsync(ms, progress, cancellationToken);
                    data = ms.ToArray();
                }
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }

            return data;
        }

        /// <summary>
        /// Copies file from local file path to Temp folder on the remote session.
        /// </summary>
        /// <param name="filePath">Path to the file to be sent to remote session.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        /// <returns>Path to the file on the remote machine.</returns>
        public async Task<string> CopyFileToRemoteTempAsync(string filePath, bool doCleanUp, IProgress<long> progress, CancellationToken cancellationToken) {
            string fileName = Path.GetFileName(filePath);
            var blobinfo = await SendFileAsync(filePath, doCleanUp, progress, cancellationToken);
            return await _session.EvaluateAsync<string>(Invariant($"rtvs:::save_to_temp_folder({blobinfo.Id}, {fileName.ToRStringLiteral()})"), REvaluationKind.Normal, cancellationToken);
        }

        /// <summary>
        /// Copies file from remote file path to downloads folder on the local session.
        /// </summary>
        /// <param name="filePath">Path to the file on remote machine.</param>
        public Task FetchFileToLocalTempAsync(string remoteRPath, string localRPath, CancellationToken cancellationToken) {
            var arg = !string.IsNullOrEmpty(localRPath) ? Invariant($"localPath = {localRPath.ToRStringLiteral()}") : "localPath = ''";
            return _session.EvaluateAsync(Invariant($"rtvs:::fetch_file({remoteRPath.ToRStringLiteral()}, {arg}, silent = TRUE)"), REvaluationKind.Normal, cancellationToken);
        }

        public void Dispose() {
            _blobService.DestroyBlobsAsync(_cleanup.Select(b => b.Id).ToArray(), CancellationToken.None).DoNotWait();
        }
    }
}
