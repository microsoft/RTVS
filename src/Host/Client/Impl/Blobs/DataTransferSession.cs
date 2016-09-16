// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.R.Host.Client {
    public class DataTransferSession : IDisposable {
        private readonly IRBlobService _blobService;
        private readonly IFileSystem _fs;
        private readonly List<IRBlobInfo> _cleanup;

        public DataTransferSession(IRBlobService service, IFileSystem fs) {
            _blobService = service;
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
        public async Task<IRBlobInfo> SendBytesAsync(byte[] data, bool doCleanUp = true) {
            IRBlobInfo blob = null;
            using (RBlobStream blobStream = await RBlobStream.CreateAsync(_blobService))
            using (Stream ms = new MemoryStream(data)) {
                await ms.CopyToAsync(blobStream);
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
        public async Task<IRBlobInfo> SendFileAsync(string filePath, bool doCleanUp = true) {
            IRBlobInfo blob = null;
            using (RBlobStream blobStream = await RBlobStream.CreateAsync(_blobService))
            using (Stream fileStream = _fs.FileOpenRead(filePath)){
                await fileStream.CopyToAsync(blobStream);
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
        public async Task FetchFileAsync(IRBlobInfo blob, string filePath, bool doCleanUp = true) {
            using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _blobService))
            using (Stream fileStream = _fs.CreateFile(filePath)) {
                await blobStream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }
        }


        /// <summary>
        /// Gets the data for a given blob from R-Host. This method adds the blob for clean up by default.
        /// </summary>
        /// <param name="blob">Blob from which the data is to be retrieved.</param>
        /// <param name="doCleanUp">true to add blob upon transfer for cleanup on dispose, false to ignore it after transfer.</param>
        public async Task<byte[]> FetchBytesAsync(IRBlobInfo blob, bool doCleanUp = true) {
            byte[] data = null;
            using (RBlobStream blobStream = await RBlobStream.OpenAsync(blob, _blobService))
            using (MemoryStream ms = new MemoryStream((int)blobStream.Length)) {
                await blobStream.CopyToAsync(ms);
                await ms.FlushAsync();
                data = ms.ToArray();
            }

            if (doCleanUp) {
                _cleanup.Add(blob);
            }

            return data;
        }

        public void Dispose() {
            _blobService.DestroyBlobsAsync(_cleanup.Select(b => b.Id).ToArray()).DoNotWait();
        }
    }
}
