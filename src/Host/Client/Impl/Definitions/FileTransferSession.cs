// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.R.Host.Client {
    public class FileTransferSession : IDisposable {
        private readonly IRSession _session;
        private readonly IFileSystem _fs;
        private List<IRBlob> _cleanup;

        public FileTransferSession(IRSession session, IFileSystem fs) {
            _session = session;
            _fs = fs;
            _cleanup = new List<IRBlob>();
        }

        /// <summary>
        /// Sends file to R-Host as a blob. This method adds the blob for clean up by default. 
        /// </summary>
        /// <param name="filePath">Path to the file to be sent to R-Host.</param>
        /// <param name="doCleanUp">true to add blob created upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task<IRBlobData> SendFileAsync(string filePath, bool doCleanUp = true) {
            byte[] bytes = _fs.FileReadAllBytes(filePath);
            long blobId = await _session.SendBlobAsync(bytes);
            var result = new BlobData(blobId, bytes);

            if (doCleanUp) {
                _cleanup.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Gets the data for a given blob from R-Host. Saves the data to <paramref name="filePath"/>. This 
        /// method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blob">Blob from which the data is to be retrieved.</param>
        /// <param name="filePath">Path to the file where the retrieved data will be written.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task FetchFileAsync(IRBlob blob, string filePath, bool doCleanUp = true) {
            var result = await _session.GetBlobAsync(new long[] { blob.Id });
            _fs.FileWriteAllBytes(filePath, result[0].Data);

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
        }

        public async void Dispose() {
            await _session.DestroyBlobAsync(_cleanup.Where(b => { return b != null; }).Select(b => b.Id)).SilenceException<OperationCanceledException>();
        }
    }
}
