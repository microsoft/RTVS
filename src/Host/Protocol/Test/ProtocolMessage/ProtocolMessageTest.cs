// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

using static Microsoft.UnitTests.Core.Random;

namespace Microsoft.R.Host.Protocol.Test.ProtocolMessage {
    [Category.FuzzTest]
    public class ProtocolMessageTest {

        [CompositeTest]
        [IntRange(1000)]
        public void RHostPipeFuzzTest(int iteration) {
            ParallelTools.Invoke(1000, i => {
                var input = GenerateInput();
                try {
                    var message = Message.Parse(input);
                    var msgbytes = message.ToBytes();
                } catch (InvalidDataException) {
                } catch (TraceFailException) {
                }
            });
        }

        private byte[] GenerateInput() {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms)) {
                // Id =uint64
                writer.Write(GenerateUInt64());
                // Request id = uint64
                writer.Write(GenerateUInt64());
                // Name
                writer.Write(GenerateAsciiString().ToCharArray());
                writer.Write('\0');
                // Json ARRAY string = {[, , , ]} 
                writer.Write(GenerateJsonArray().ToCharArray());
                writer.Write('\0');
                // blob
                writer.Write(GenerateBytes());
                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
