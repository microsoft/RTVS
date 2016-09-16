// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Broker.Pipes {
    public static class MessageParser {
        public static ulong GetId(byte[] message) =>
            BitConverter.ToUInt64(message, 0);

        public static ulong GetRequestId(byte[] message) =>
            BitConverter.ToUInt64(message, 8);

        public static bool IsNamed(byte[] message, byte[] name) {
            int i = 16;

            if (i + name.Length >= message.Length) {
                return false;
            }

            foreach (var ch in name) {
                if (message[i++] != ch) {
                    return false;
                }
            }

            return true;
        }
    }
}
