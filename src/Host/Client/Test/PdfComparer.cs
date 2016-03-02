// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using FluentAssertions;

namespace Microsoft.R.Host.Client.Test {
    static class PdfComparer {
        /// <summary>
        /// Compare 2 PDF files, ignoring the headers.
        /// </summary>
        /// <param name="actualFilePath">PDF file to validate.</param>
        /// <param name="expectedFilePath">PDF file to compare against.</param>
        public static void ComparePdfFiles(string actualFilePath, string expectedFilePath) {
            var actualPdfBytes = File.ReadAllBytes(actualFilePath);
            var expectedPdfBytes = File.ReadAllBytes(expectedFilePath);
            ClearPdfHeader(actualPdfBytes);
            ClearPdfHeader(expectedPdfBytes);

            actualPdfBytes.Should().Equal(expectedPdfBytes);
        }

        private static void ClearPdfHeader(byte[] data) {
            // PDF files have headers that include CreationDate, ModDate, Producer
            // which will not match for 2 identical exports of plots
            // That metadata is stored in the first object, between '<<' and '>>'
            int startMetadataIndex = IndexOfSequence(data, new byte[] { 0x3c, 0x3c }, 0);
            startMetadataIndex.Should().BeGreaterOrEqualTo(0);
            int endMetadataIndex = IndexOfSequence(data, new byte[] { 0x3e, 0x3e }, startMetadataIndex);
            endMetadataIndex.Should().BeGreaterThan(startMetadataIndex);

            // Replace the whole metadata block with spaces
            for (int i = startMetadataIndex + 2; i < endMetadataIndex; i++) {
                data[i] = 0x20;
            }
        }

        private static int IndexOfSequence(byte[] data, byte[] sequence, int startIndex) {
            int index = startIndex;
            while (index < data.Length - sequence.Length) {
                var dataSegment = new ArraySegment<byte>(data, index, sequence.Length);
                if (dataSegment.SequenceEqual(sequence)) {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}
