// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    public class RBlobStream : Stream {
        private readonly IRBlobInfo _blob;
        private readonly IRBlobService _blobService;
        private bool _isDisposed;

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = true;

        private bool _canWrite;
        public override bool CanWrite {
            get {
                return _canWrite;
            }
        }

        private long _length;
        public override long Length {
            get {
                return _length;
            }
        }

        private RBlobStream(IRBlobInfo blob, bool canWrite, IRBlobService blobService) {
            _blob = blob;
            _canWrite = canWrite;
            _blobService = blobService;
            _isDisposed = false;
        }

        private long _position;
        public override long Position {
            get {
                return _position;
            }
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush() => _length = _blobService.GetBlobSizeAsync(_blob.Id).GetAwaiter().GetResult();

        public override int Read(byte[] buffer, int offset, int count) {
            byte[] bytes = _blobService.BlobReadAsync(_blob.Id, Position, count).GetAwaiter().GetResult();
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
            _position += bytes.Length;
            return bytes.Length;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            long temp = _position;
            switch (origin) {
                case SeekOrigin.Begin:
                    temp = offset;
                    break;
                case SeekOrigin.Current:
                    temp = _position + offset;
                    break;
                case SeekOrigin.End:
                    temp = _length - offset;
                    break;
            }

            // make sure position satisfies 0 <= position <= _length
            if (temp < 0) {
                temp = 0;
            } else if (temp > _length) {
                temp = _length;
            }

            _position = temp;
            return _position;
        }

        public override void SetLength(long value) {
            _blobService.SetBlobSizeAsync(_blob.Id, value).GetAwaiter().GetResult();
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            byte[] bytesToSend = null;
            if (offset != 0 || count != buffer.Length) {
                bytesToSend = new byte[count];
                Array.Copy(buffer, offset, bytesToSend, 0, count);
            } else {
                bytesToSend = buffer;
            }

            _length = _blobService.BlobWriteAsync(_blob.Id, bytesToSend, _position).GetAwaiter().GetResult();
            _position += count;
        }

        protected override void Dispose(bool disposing) {
            if (!disposing && !_isDisposed) {
                _isDisposed = true;
            }
        }

        public IRBlobInfo GetBlobInfo() => _blob;

        private void ThrowIfDisposed([CallerMemberName] string callerName = "") {
            if (_isDisposed) {
                throw new ObjectDisposedException(
                    Invariant($"{nameof(RBlobStream)} for Blob {_blob.Id} has been disposed, cannot call '{callerName}' after disposing."));
            }
        }

        public static RBlobStream Create(IRBlobService blobService) => CreateAsync(blobService).GetAwaiter().GetResult();

        public static async Task<RBlobStream> CreateAsync(IRBlobService blobService, CancellationToken ct = default(CancellationToken)) {
            var blobId = await blobService.CreateBlobAsync(ct);
            return new RBlobStream(new RBlobInfo(blobId), true, blobService);
        }

        public static RBlobStream Open(IRBlobInfo blobInfo, IRBlobService blobService) => new RBlobStream(blobInfo, false, blobService);

        public static Task<RBlobStream> OpenAsync(IRBlobInfo blobInfo, IRBlobService blobService, CancellationToken ct = default(CancellationToken)) 
            => Task.FromResult(Open(blobInfo, blobService));

        public static void Destroy(IRBlobInfo blobInfo, IRBlobService blobService) => blobService.DestroyBlobsAsync(new ulong[] { blobInfo.Id }).GetAwaiter().GetResult();

        public static Task DestroyAsync(IRBlobInfo blobInfo, IRBlobService blobService, CancellationToken ct = default(CancellationToken)) 
            => blobService.DestroyBlobsAsync(new [] { blobInfo.Id }, ct);
    }
}
