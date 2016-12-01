// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    [ThreadType(ThreadType.Background)]
    public class AsyncReaderWriterLockTest {
        private readonly AsyncReaderWriterLock _arwl;

        public AsyncReaderWriterLockTest() {
            _arwl = new AsyncReaderWriterLock();
        }

        [Test]
        public void ReadRead() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadRead_FirstCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);

            task1.Should().BeCanceled();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadRead_SecondCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
        }

        [Test]
        public void ReadRead_CancelFirst() {
            var cts = new CancellationTokenSource();
            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadRead_CancelSecond() {
            var cts = new CancellationTokenSource();
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadWrite() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.WriterLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWrite_FirstCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeCanceled();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadWrite_SecondCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
        }

        [Test]
        public void ReadWrite_CancelFirst() {
            var cts = new CancellationTokenSource();
            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWrite_CancelSecond() {
            var cts = new CancellationTokenSource();
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
        }

        [Test]
        public void ReadWrite_CancelSecond_ReadWrite_ReleaseThird() {
            var cts = new CancellationTokenSource();
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);

            cts.Cancel();
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_FirstCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeCanceled();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_SecondCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_FirstSecondCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.ReaderLockAsync(cts.Token);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeCanceled();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadReadWrite_CancelFirst() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_CancelSecond() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_CancelFirstSecond() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(cts.Token);
            var task2 = _arwl.ReaderLockAsync(cts.Token);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_Release() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWrite_CancelThird_Release() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);

            task1.Result.Dispose();
            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
        }

        [Test]
        public void ReadReadWrite_Release_CancelThird() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
        }

        [Test]
        public void ReadReadWrite_Release_Release() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();

            task1.Result.Dispose();
            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadReadWrite_Release_CancelThird_Release() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);

            task1.Result.Dispose();
            cts.Cancel();
            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
        }

        [Test]
        public void ReadReadWrite_Release_Release_CancelThird() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);

            task1.Result.Dispose();
            task2.Result.Dispose();
            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadWriteRead() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.WriterLockAsync();
            var task3 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWriteRead_CancelSecond_Write() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);
            
            cts.Cancel();
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWriteRead_CancelSecond_Write_ReleaseThird() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);
            
            cts.Cancel();
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);
            task3.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWriteRead_CancelSecond_Write_ReleaseFirst_ReleaseThird() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);
            
            cts.Cancel();
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);
            task1.Result.Dispose();
            task3.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadWriteRead_CancelThird_ReleaseFirst() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None);
            var task3 = _arwl.ReaderLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
        }

        [Test]
        public void ReadWriteWrite_CancelThird_Read_CancelSecond() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts2.Token);
            var task3 = _arwl.WriterLockAsync(cts3.Token);
            
            cts3.Cancel();
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);
            cts2.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadWriteWrite_CancelThird_ReadWrite_CancelSecond() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts2.Token);
            var task3 = _arwl.WriterLockAsync(cts3.Token);
            
            cts3.Cancel();
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task5 = _arwl.WriterLockAsync(CancellationToken.None);
            cts2.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWriteRead() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();
            var task4 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
            task4.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWriteRead_WriteCanceled() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadReadWriteRead_CancelWrite() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadReadWriteRead_Release_Release() {
            var task1 = _arwl.ReaderLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();
            var task4 = _arwl.ReaderLockAsync();

            task1.Result.Dispose();
            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
        }

        [Test]
        public void ReadReadWriteRead_CancelWrite_Release_Release() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts.Cancel();
            task1.Result.Dispose();
            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadReadWriteWrite_CancelWrite_Release_Release() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();
            task1.Result.Dispose();
            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadReadWriteReadWrite_CancelWrite() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task5 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWriteReadWrite_CancelSecond() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
            task4.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWriteReadWrite_CancelSecond_ReleaseFirst() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
            task4.Should().BeRanToCompletion();
        }
        
        [Test]
        public void ReadWriteReadWriteRead_CancelThird_ReleaseFirst() {
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None);
            var task3 = _arwl.ReaderLockAsync(cts3.Token);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);
            var task5 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts3.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void ReadWriteWriteRead_CancelSecond_CancelThird() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts2.Token);
            var task3 = _arwl.WriterLockAsync(cts3.Token);
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts2.Cancel();
            cts3.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ReadWriteWriteRead_CancelThird_CancelSecond() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts2.Token);
            var task3 = _arwl.WriterLockAsync(cts3.Token);
            var task4 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts3.Cancel();
            cts2.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteReadReadWriteRead() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.ReaderLockAsync();
            var task4 = _arwl.WriterLockAsync();
            var task5 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void WriteReadReadWriteRead_Release() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.ReaderLockAsync();
            var task4 = _arwl.WriterLockAsync();
            var task5 = _arwl.ReaderLockAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
            task4.Should().BeRanToCompletion();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void WriteReadReadWriteRead_CancelThird_CancelSecond_Release() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts2.Token);
            var task3 = _arwl.ReaderLockAsync(cts3.Token);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);
            var task5 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts3.Cancel();
            cts2.Cancel();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void WriteRead() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
        }
        
        [Test]
        public void WriteRead_CancelSecond_ReleaseFirst_Read() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteRead_CancelSecond_ReleaseFirst_Write() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void Write_Release_Read() {
            var task1 = _arwl.WriterLockAsync();
            task1.Result.Dispose();
            var task2 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteReadWrite() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void WriteReadWrite_Release() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteWriteRead_CancelSecond() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void WriteWriteRead_CancelSecond_ReleaseFirst() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }
        
        [Test]
        public void WriteWriteReadWrite_CancelSecond_CancelThird_ReleaseFirst() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts2.Token);
            var task3 = _arwl.ReaderLockAsync(cts3.Token);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            cts2.Cancel();
            cts3.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteWriteReadWrite_CancelThird_CancelSecond_ReleaseFirst() {
            var cts2 = new CancellationTokenSource();
            var cts3 = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts2.Token);
            var task3 = _arwl.ReaderLockAsync(cts3.Token);
            var task4 = _arwl.WriterLockAsync(CancellationToken.None);

            cts3.Cancel();
            cts2.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeCanceled();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteReadWrite_CancelThird_ReleaseFirst() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None);
            var task3 = _arwl.WriterLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeCanceled();
        }

        [Test]
        public void WriteReadWrite_Release_Write_Release() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = _arwl.WriterLockAsync();

            task1.Result.Dispose();
            var task4 = _arwl.WriterLockAsync();
            task3.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().BeRanToCompletion();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteWrite() {
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.WriterLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
        }
        
        [Test]
        public void WriteWrite_CancelSecond_ReleaseFirst_Read() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteWrite_CancelSecond_ReleaseFirst_Write() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void Write_Release_Write() {
            var task1 = _arwl.WriterLockAsync();
            task1.Result.Dispose();
            var task2 = _arwl.WriterLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void WriteWriteWrite_CancelSecond() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void WriteWriteWrite_CancelSecond_ReleaseFirst() {
            var cts = new CancellationTokenSource();

            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public async Task Concurrent_Write() {
            var writersCount = 0;
            await ParallelTools.InvokeAsync(12, async i => {
                using (await _arwl.WriterLockAsync()) {
                    var count = writersCount;
                    await Task.Delay(20);
                    // CompareExchange won't work it lock wasn't exclusive
                    Interlocked.CompareExchange(ref writersCount, count + 1, count);
                }
            });

            writersCount.Should().Be(12);
        }

        [Test]
        public async Task Concurrent_Write_ReadCanceled() {
            var writersCount = 0;

            await ParallelTools.InvokeAsync(48, async i => {
                // Every 4th lock is writer lock
                var isWriter = i % 4 == 0;
                if (isWriter) {
                    var task = _arwl.WriterLockAsync(CancellationToken.None);
                    using (await task) {
                        var count = writersCount;
                        await Task.Delay(20, CancellationToken.None);
                        // CompareExchange won't work if lock wasn't exclusive
                        Interlocked.CompareExchange(ref writersCount, count + 1, count);
                    }
                    return task;
                } else {
                    var cts = new CancellationTokenSource();
                    var task = _arwl.ReaderLockAsync(cts.Token);
                    cts.Cancel();
                    if (!task.IsCanceled) {
                        (await task).Dispose();
                    }
                    return task;
                }
            });

            writersCount.Should().Be(12);
        }

        [Test]
        public async Task Concurrent_ReadWrite() {
            var writersCount = 0;
            await ParallelTools.InvokeAsync(48, async i => {
                // Every 4th lock is writer lock
                var isWriter = i % 4 == 3;
                if (isWriter) {
                    using (await _arwl.WriterLockAsync()) {
                        var count = writersCount;
                        await Task.Delay(20);
                        // CompareExchange won't work it lock wasn't exclusive
                        Interlocked.CompareExchange(ref writersCount, count + 1, count);
                    }
                } else {
                    using (await _arwl.ReaderLockAsync()) { }
                }
            }, 50000);

            writersCount.Should().Be(12);
        }

        [Test]
        public async Task Concurrent_ReadWriteRead_CancelSecond() {
            var task = _arwl.ReaderLockAsync(CancellationToken.None);
            var tasks = await ParallelTools.InvokeAsync(12, i => {
                var cts = new CancellationTokenSource();
                var task1 = _arwl.WriterLockAsync(cts.Token);
                var task2 = _arwl.ReaderLockAsync(CancellationToken.None);
                cts.Cancel();
                return new[] { task1, task2 };
            }, Task.WhenAll);

            task.Should().BeRanToCompletion();
            foreach (var taskPair in tasks) {
                taskPair[0].Should().BeCanceled();
                taskPair[1].Should().BeRanToCompletion();
            }
        }

        [Test]
        public async Task Concurrent_ReadExclusiveRead_CancelSecond() {
            var tasks = await ParallelTools.InvokeAsync(48, i => {
                // Every 4th lock is writer lock
                var isExclusiveReader = i % 2 == 1;
                if (isExclusiveReader) {
                    var erl = _arwl.CreateExclusiveReaderLock();
                    return erl.WaitAsync();
                }

                return _arwl.ReaderLockAsync();
            }, 50000);

            tasks.Should().OnlyContain(t => t.Status == TaskStatus.RanToCompletion);
        }

        [Test]
        public void Reentrancy_ReadRead() {
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        } 

        [Test]
        public void Reentrancy_ReadReadWrite() {
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None, task1.Result.Reentrancy);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        } 

        [Test]
        public void Reentrancy_ReadWrite() {
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
        } 

        [Test]
        public void Reentrancy_ReadWriteRead() {
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void Reentrancy_ReadWriteWrite() {
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().NotBeCompleted();
        } 

        [Test]
        public void Reentrancy_ReadWriteWrite_ReleaseFirst() {
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        } 

        [Test]
        public void Reentrancy_WriteRead() {
            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        } 

        [Test]
        public void Reentrancy_WriteReadWrite() {
            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(CancellationToken.None, task1.Result.Reentrancy);
            var task3 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void Reentrancy_WriteWrite() {
            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        } 

        [Test]
        public void Reentrancy_WriteWriteRead() {
            var task1 = _arwl.WriterLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(CancellationToken.None, task1.Result.Reentrancy);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None, task1.Result.Reentrancy);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void Read_ExclusiveRead() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.ReaderLockAsync();
            var task2 = erl.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }

        [Test]
        public void Read_ExclusiveRead_Write_ExclusiveRead_ReleaseSecond() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.ReaderLockAsync();
            var task2 = erl.WaitAsync();
            var task3 = _arwl.WriterLockAsync();
            var task4 = erl.WaitAsync();

            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
            task4.Should().BeRanToCompletion();
        }

        [Test]
        public void ExclusiveRead_Read() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync();
            var task2 = _arwl.ReaderLockAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        }
        
        [Test]
        public void ExclusiveRead_ExclusiveRead() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync();
            var task2 = erl.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
        }
        
        [Test]
        public void ExclusiveRead_Read_ExclusiveRead_CancelSecond() {
            var cts = new CancellationTokenSource();
            
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = erl.WaitAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ExclusiveRead_Read_ReleaseFirst_ExclusiveRead() {
            var cts = new CancellationTokenSource();
            
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = _arwl.ReaderLockAsync(cts.Token);

            task1.Result.Dispose();

            var task3 = erl.WaitAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void ExclusiveRead_ExclusiveRead_ExclusiveRead_CancelSecond() {
            var cts = new CancellationTokenSource();

            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = erl.WaitAsync(cts.Token);
            var task3 = erl.WaitAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
        }
        
        [Test]
        public void ExclusiveRead_ExclusiveRead_ExclusiveRead_ReleaseFirst() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync();
            var task2 = erl.WaitAsync();
            var task3 = erl.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        }
        
        [Test]
        public void ExclusiveRead_ExclusiveRead_ExclusiveRead_CancelSecond_ReleaseFirst() {
            var cts = new CancellationTokenSource();

            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = erl.WaitAsync(cts.Token);
            var task3 = erl.WaitAsync(CancellationToken.None);

            cts.Cancel();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }
        
        [Test]
        public void ExclusiveRead_ExclusiveRead_CancelSecond_ReleaseFirst_ExclusiveRead() {
            var cts = new CancellationTokenSource();
            
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = erl.WaitAsync(cts.Token);

            cts.Cancel();
            task1.Result.Dispose();

            var task3 = erl.WaitAsync(CancellationToken.None);

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void ExclusiveRead_SecondExclusiveRead_SecondExclusiveRead_Write_ReleaseSecond() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = erl1.WaitAsync();
            var task2 = erl2.WaitAsync();
            var task3 = erl2.WaitAsync();
            var task4 = _arwl.WriterLockAsync();

            task2.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
        } 

        [Test]
        public void ExclusiveRead_SecondExclusiveRead_ExclusiveRead_ReleaseSecond_ReleaseFirst() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = erl1.WaitAsync();
            var task2 = erl2.WaitAsync();
            var task3 = erl1.WaitAsync();

            task2.Result.Dispose();
            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void ExclusiveRead_SecondExclusiveRead_ReleaseSecond_ReleaseFirst_ExclusiveRead() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = erl1.WaitAsync();
            var task2 = erl2.WaitAsync();

            task2.Result.Dispose();
            task1.Result.Dispose();

            var task3 = erl1.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void ExclusiveRead_SecondExclusiveRead_ReleaseSecond_ReleaseFirst_SecondExclusiveRead() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = erl1.WaitAsync();
            var task2 = erl2.WaitAsync();

            task2.Result.Dispose();
            task1.Result.Dispose();

            var task3 = erl2.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void ExclusiveRead_SecondExclusiveRead_ExclusiveRead_SecondExclusiveRead() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = erl1.WaitAsync();
            var task2 = erl2.WaitAsync();
            var task3 = erl1.WaitAsync();
            var task4 = erl2.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
            task4.Should().NotBeCompleted();
        } 

        [Test]
        public void ExclusiveRead_ExclusiveRead_SecondExclusiveRead_SecondExclusiveRead() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = erl1.WaitAsync();
            var task2 = erl1.WaitAsync();
            var task3 = erl2.WaitAsync();
            var task4 = erl2.WaitAsync();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
        } 

        [Test]
        public void ExclusiveRead_ExclusiveRead_ReleaseFirst() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync();
            var task2 = erl.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
        } 

        [Test]
        public void Read_Write_ExclusiveRead_CancelSecond() {
            var cts = new CancellationTokenSource();
            
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.ReaderLockAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = erl.WaitAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void WriteExclusiveReadRead_ReleaseFirst() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.WriterLockAsync();
            var task2 = erl.WaitAsync();
            var task3 = _arwl.ReaderLockAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void WriteReadExclusiveRead_ReleaseFirst() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.WriterLockAsync();
            var task2 = _arwl.ReaderLockAsync();
            var task3 = erl.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void WriteExclusiveReadExclusiveRead_ReleaseFirst() {
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.WriterLockAsync();
            var task2 = erl.WaitAsync();
            var task3 = erl.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().NotBeCompleted();
        } 

        [Test]
        public void WriteExclusiveRead_SecondExclusiveRead_ReleaseFirst() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.WriterLockAsync();
            var task2 = erl1.WaitAsync();
            var task3 = erl2.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
        } 

        [Test]
        public void Write_ExclusiveRead_SecondExclusiveRead_Read_ReleaseFirst() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.WriterLockAsync();
            var task2 = erl1.WaitAsync();
            var task3 = erl2.WaitAsync();
            var task4 = _arwl.ReaderLockAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
            task4.Should().BeRanToCompletion();
        } 

        [Test]
        public void Write_ExclusiveRead_SecondExclusiveRead_SecondExclusiveRead_ExclusiveRead_ReleaseFirst() {
            var erl1 = _arwl.CreateExclusiveReaderLock();
            var erl2 = _arwl.CreateExclusiveReaderLock();
            var task1 = _arwl.WriterLockAsync();
            var task2 = erl1.WaitAsync();
            var task3 = erl2.WaitAsync();
            var task4 = erl2.WaitAsync();
            var task5 = erl1.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().BeRanToCompletion();
            task3.Should().BeRanToCompletion();
            task4.Should().NotBeCompleted();
            task5.Should().NotBeCompleted();
        }

        [Test]
        public void ExclusiveRead_Write_ExclusiveRead_ReleaseFirst() {
            var erl = _arwl.CreateExclusiveReaderLock();

            var task1 = erl.WaitAsync();
            var task2 = _arwl.WriterLockAsync();
            var task3 = erl.WaitAsync();

            task1.Result.Dispose();

            task1.Should().BeRanToCompletion();
            task2.Should().NotBeCompleted();
            task3.Should().BeRanToCompletion();
        }

        [Test]
        public void ExclusiveRead_Write_ExclusiveRead_CancelSecond() {
            var cts = new CancellationTokenSource();
            
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = erl.WaitAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().NotBeCompleted();
        }

        [Test]
        public void ExclusiveRead_Write_Read_CancelSecond() {
            var cts = new CancellationTokenSource();
            
            var erl = _arwl.CreateExclusiveReaderLock();
            var task1 = erl.WaitAsync(CancellationToken.None);
            var task2 = _arwl.WriterLockAsync(cts.Token);
            var task3 = _arwl.ReaderLockAsync(CancellationToken.None);

            cts.Cancel();

            task1.Should().BeRanToCompletion();
            task2.Should().BeCanceled();
            task3.Should().BeRanToCompletion();
        }
    }
}