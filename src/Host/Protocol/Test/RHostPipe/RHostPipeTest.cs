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
using Microsoft.UnitTests.Core;
using Microsoft.UnitTests.Core.XUnit;

using static System.FormattableString;
using static Microsoft.UnitTests.Core.Random;

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

            // Process should not exit before fully starting up
            process.HasExited.Should().BeFalse();
            try {

                Task killRhostTask = Task.Delay(3000).ContinueWith(t => TryKill(process));

                CancellationTokenSource cts = new CancellationTokenSource(3000);
                await ReadStartupOutputFromRHostAsync(process.StandardOutput.BaseStream, cts.Token);
                await SendFuzzedInputToRHostAsync(process.StandardInput.BaseStream, input, cts.Token);
                Task readOutputTask = ReadAnyOutputFromRHostAsync(process.StandardOutput.BaseStream, cts.Token);
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
            try {
                // Capture R messages after sending fuzzed input.
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead = 0;
                do {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                } while (!ct.IsCancellationRequested);
            } catch(Exception ex) when (!ex.IsCriticalException()) { }
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
    }
}
