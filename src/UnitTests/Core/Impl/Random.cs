// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;

namespace Microsoft.UnitTests.Core {
    public class Random {
        public static string GenerateAsciiString(int max = 10000) {
            byte[] rbuf = new byte[_rand.Next(0, max)];
            _rand.NextBytes(rbuf);
            return Encoding.ASCII.GetString(rbuf);
        }

        public static string GenerateUTF8String(int max = 10000) {
            byte[] rbuf = new byte[_rand.Next(0, max)];
            _rand.NextBytes(rbuf);
            return Encoding.UTF8.GetString(rbuf);
        }

        public static string GenerateJsonArray() {
            if (_rand.Next() % 2 == 0) {
                // return random UTF8 string
                return GenerateUTF8String();
            }

            string[] parts = new string[_rand.Next(0, 1000)];
            for (int i = 0; i < parts.Length; ++i) {
                parts[i] = GenerateUTF8String(100);
            }

            // return a JSON array of random UTF8 strings
            return "{[" + string.Join(",", parts) + "]}";
        }

        public static byte[] GenerateBytes(int max = 1000000) {
            byte[] rbuf = new byte[_rand.Next(0, max)];
            _rand.NextBytes(rbuf);
            return rbuf;
        }

        public static ulong GenerateUInt64() {
            byte[] rbuf = new byte[8];
            _rand.NextBytes(rbuf);
            return BitConverter.ToUInt64(rbuf, 0);
        }

        private static System.Random _rand = new System.Random();
    }
}
