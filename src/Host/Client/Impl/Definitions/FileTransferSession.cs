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
        private readonly List<IRBlobInfo> _cleanup;

        public FileTransferSession(IRSession session, IFileSystem fs) {
            _session = session;
            _fs = fs;
            _cleanup = new List<IRBlobInfo>();
        }

        /// <summary>
        /// Sends file to R-Host as a blob. This method adds the blob for clean up by default. 
        /// </summary>
        /// <param name="filePath">Path to the file to be sent to R-Host.</param>
        /// <param name="doCleanUp">true to add blob created upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task<IRBlobInfo> SendFileAsync(string filePath, bool doCleanUp = true) {
            byte[] bytes = _fs.FileReadAllBytes(filePath);
            ulong blobId = await _session.CreateBlobAsync(bytes);
            var result = new Blob(blobId, bytes);

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
        public async Task FetchFileAsync(IRBlobInfo blob, string filePath, bool doCleanUp = true) {
            var data = await _session.GetBlobAsync(blob.Id);
            _fs.FileWriteAllBytes(filePath, data);

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
        }

        public void Dispose() {
            _session.DestroyBlobAsync(_cleanup.Select(b => b.Id).ToArray()).DoNotWait();
        }
    }
}
