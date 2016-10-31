// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.XUnit;

using static System.FormattableString;

namespace Microsoft.R.Host.Protocol.Test.RHostPipe {
    public class RHostPipeTest {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        internal class Interpreter {
            public string Path { get; set; }
            public string BinPath { get; set; }
        }

        private IEnumerable<Interpreter> GetInterpreters() {
            var engines = new RInstallation().GetCompatibleEngines();
            if (engines.Any()) {
                foreach (var e in engines) {
                    var detected = new Interpreter() { Path = e.InstallPath, BinPath = e.BinPath };
                    yield return detected;
                }
            } 
        }

        private async Task RHostPipeFuzzTestRunnerAsync(byte[] input) {
            string RHostExe = "Microsoft.R.Host.exe";

            string brokerPath = Path.GetDirectoryName(typeof(RHostPipeTest).Assembly.GetAssemblyPath());
            string rhostExePath = Path.Combine(brokerPath, RHostExe);
            string arguments = Invariant($"--rhost-name \"FuzzTest\"");

            ProcessStartInfo psi = new ProcessStartInfo(rhostExePath) {
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = true
            };
            var shortHome = new StringBuilder(NativeMethods.MAX_PATH);
            GetShortPathName(_interpreter.Path, shortHome, shortHome.Capacity);
            psi.EnvironmentVariables["R_HOME"] = shortHome.ToString();
            psi.EnvironmentVariables["PATH"] = _interpreter.BinPath + ";" + Environment.GetEnvironmentVariable("PATH");

            psi.WorkingDirectory = Path.GetDirectoryName(rhostExePath);

            Process process = new Process {
                StartInfo = psi,
                EnableRaisingEvents = true,
            };

            process.Start();
            process.WaitForExit(250);
            if (process.HasExited && process.ExitCode < 0) {
                return;
            }

            CancellationTokenSource cts = new CancellationTokenSource(3000);
            Task.Run(() => {
                cts.Token.WaitHandle.WaitOne();
                TryKill(process);
            }).DoNotWait();

            await ReadStartupOutputFromRHostAsync(process.StandardOutput.BaseStream, cts.Token);
            await SendFuzzedInputToRHostAsync(process.StandardInput.BaseStream, input, cts.Token);
            ReadAnyOutputFromRHostAsync(process.StandardOutput.BaseStream, cts.Token).DoNotWait();

            // Fuzzed input should not kill the host.
            process.HasExited.Should().BeFalse();

            TryKill(process);
        }

        private static void TryKill(Process process) {
            try {
                process.Kill();
            } catch (Exception) { }
        }

        private async Task ReadStartupOutputFromRHostAsync(Stream stream, CancellationToken ct) {
            // Capture R Startup messages.
            byte[] buffer = new byte[1024*1024];
            int bytesRead = 0;
            do {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            } while (bytesRead == buffer.Length && !ct.IsCancellationRequested);
        }

        private async Task SendFuzzedInputToRHostAsync(Stream stream, byte[] input, CancellationToken ct) {
            await stream.WriteAsync(input, 0, input.Length, ct);
        }

        private async Task ReadAnyOutputFromRHostAsync(Stream stream, CancellationToken ct) {
            // Capture R messages after sending fuzzed input.
            byte[] buffer = new byte[1024 * 1024];
            int bytesRead = 0;
            do {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            } while (!ct.IsCancellationRequested);
        }

        private Interpreter _interpreter;

        [Test]
        [Category.FuzzTest]
        public async Task RHostPipeFuzzTest() {
            var interpreters = GetInterpreters();
            _interpreter = interpreters.FirstOrDefault();

            for (int i = 0; i < 100; ++i) {
                byte[] input = GenerateInput();
                await RHostPipeFuzzTestRunnerAsync(input);
            }
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

        private string GenerateAsciiString(int max = 10000) {
            byte[] rbuf = new byte[_rand.Next(0, max)];
            _rand.NextBytes(rbuf);
            return Encoding.ASCII.GetString(rbuf);
        }

        private string GenerateUTF8String(int max = 10000) {
            byte[] rbuf = new byte[_rand.Next(0, max)];
            _rand.NextBytes(rbuf);
            return Encoding.UTF8.GetString(rbuf);
        }

        private string GenerateJsonArray() {
            if(_rand.Next()%2 == 0) {
                // return random unicode string
                return GenerateUTF8String();
            }

            string[] parts = new string[_rand.Next(0,1000)]; 
            for(int i = 0; i < parts.Length; ++i) {
                parts[i] = GenerateUTF8String(100);
            }

            // return a JSON array of random unicode strings
            return "{[" + string.Join(",", parts) + "]}";
        }

        private byte[] GenerateBytes(int max = 1000000) {
            byte[] rbuf = new byte[_rand.Next(0, max)];
            _rand.NextBytes(rbuf);
            return rbuf;
        }

        private ulong GenerateUInt64() {
            byte[] rbuf = new byte[8];
            _rand.NextBytes(rbuf);
            return BitConverter.ToUInt64(rbuf, 0);
        }

        private static Random _rand = new Random();
    }
}
