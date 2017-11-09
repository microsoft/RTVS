// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Platform.Host;
using Microsoft.R.Platform.Interpreters;
using Microsoft.R.Platform.IO;
using Microsoft.UnitTests.Core.XUnit;

using static System.FormattableString;
using static Microsoft.UnitTests.Core.Random;

namespace Microsoft.R.Host.Protocol.Test.RHostPipe {
    [Category.FuzzTest]
    public class RHostPipeTest {
        private readonly Interpreter _interpreter;

        public RHostPipeTest() {
            _interpreter = new RInstallation().GetCompatibleEngines()
                .Select(e => new Interpreter { Path = e.InstallPath, BinPath = e.BinPath })
                .FirstOrDefault();
        }
        
        [CompositeTest]
        [IntRange(100)]
        public async Task RHostPipeFuzzTest(int iteration) {
            var input = GenerateInput();

            var locator = BrokerExecutableLocator.Create(new WindowsFileSystem());
            var rhostExePath = locator.GetHostExecutablePath();
            var arguments = Invariant($"--rhost-name \"FuzzTest\" --rhost-r-dir \"{_interpreter.BinPath}\"");

            var psi = new ProcessStartInfo(rhostExePath) {
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = true
            };
            var shortHome = new StringBuilder(1024);
            GetShortPathName(_interpreter.Path, shortHome, shortHome.Capacity);
            psi.EnvironmentVariables["R_HOME"] = shortHome.ToString();
            psi.EnvironmentVariables["PATH"] = _interpreter.BinPath + ";" + Environment.GetEnvironmentVariable("PATH");

            psi.WorkingDirectory = Path.GetDirectoryName(rhostExePath);

            var process = new Process {
                StartInfo = psi,
                EnableRaisingEvents = true,
            };

            process.Start();
            process.WaitForExit(250);

            // Process should not exit before fully starting up
            process.HasExited.Should().BeFalse();
            try {

                var killRhostTask = Task.Delay(3000).ContinueWith(t => TryKill(process));

                var cts = new CancellationTokenSource(3000);
                await ReadStartupOutputFromRHostAsync(process.StandardOutput.BaseStream, cts.Token);
                await SendFuzzedInputToRHostAsync(process.StandardInput.BaseStream, input, cts.Token);
                var readOutputTask = ReadAnyOutputFromRHostAsync(process.StandardOutput.BaseStream, cts.Token);
                // Fuzzed input should not kill the host.
                process.HasExited.Should().BeFalse();

                await Task.WhenAll(killRhostTask, readOutputTask);
            } finally {
                // In case anything fails above kill host.
                TryKill(process);
            }
        }

        private static void TryKill(Process process) {
            try {
                process.Kill();
            } catch (Exception) { }
        }

        private async Task ReadStartupOutputFromRHostAsync(Stream stream, CancellationToken ct) {
            // Capture R Startup messages.
            var buffer = new byte[1024*1024];
            var bytesRead = 0;
            do {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            } while (bytesRead == buffer.Length && !ct.IsCancellationRequested);
        }

        private async Task SendFuzzedInputToRHostAsync(Stream stream, byte[] input, CancellationToken ct) {
            await stream.WriteAsync(input, 0, input.Length, ct);
        }

        private async Task ReadAnyOutputFromRHostAsync(Stream stream, CancellationToken ct) {
            try {
                // Capture R messages after sending fuzzed input.
                var buffer = new byte[1024 * 1024];
                var bytesRead = 0;
                do {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                } while (!ct.IsCancellationRequested);
            } catch(Exception ex) when (!ex.IsCriticalException()) { }
        }

        private byte[] GenerateInput() {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms)) {
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        private class Interpreter {
            public string Path { get; set; }
            public string BinPath { get; set; }
        }
    }
}
