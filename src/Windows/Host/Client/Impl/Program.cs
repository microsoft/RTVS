// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    class Program : IRCallbacks {
        private static IRExpressionEvaluator _evaluator;
        private int _nesting;

        static void Main(string[] args) {
            Console.CancelKeyPress += Console_CancelKeyPress;

            var programName = "Microsoft.R.Host.Client.Program";
            using (var shell = new CoreShell(programName)) {
                var localConnector = new LocalBrokerClient(programName, BrokerConnectionInfo.Create(null, "local", args[0], null, false), shell.Services, new NullConsole());
                var host = localConnector.ConnectAsync(new HostConnectionInfo(programName, new Program())).GetAwaiter().GetResult();
                _evaluator = host;
                host.Run().GetAwaiter().GetResult();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) { }

        public void Dispose() { }
        public Task Busy(bool which, CancellationToken ct) => Task.FromResult(true);
        public Task Connected(string rVersion) => Task.CompletedTask;
        public Task Disconnected() => Task.CompletedTask;
        public Task Shutdown(bool savedRData) => Task.CompletedTask;
        public async Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, CancellationToken ct)
            => await ReadLineAsync(prompt, ct) + "\n";

        public async Task ShowMessage(string s, CancellationToken ct) => await Console.Error.WriteLineAsync(s);

        public async Task WriteConsoleEx(string buf, OutputType otype, CancellationToken ct) {
            var writer = otype == OutputType.Output ? Console.Out : Console.Error;
            await writer.WriteAsync(buf);
        }

        /// <summary>
        /// Called as a result of R calling R API 'YesNoCancel' callback
        /// </summary>
        /// <returns>Codes that match constants in RApi.h</returns>
        public async Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, CancellationToken ct) {
            MessageButtons buttons = await ShowDialog(contexts, s, MessageButtons.YesNoCancel, ct);
            switch (buttons) {
                case MessageButtons.No:
                    return Client.YesNoCancel.No;
                case MessageButtons.Cancel:
                    return Client.YesNoCancel.Cancel;
            }
            return Client.YesNoCancel.Yes;
        }

        /// <summary>
        /// Called when R wants to display generic Windows MessageBox. 
        /// Graph app may call Win32 API directly rather than going via R API callbacks.
        /// </summary>
        /// <returns>Pressed button code</returns>
        public async Task<MessageButtons> ShowDialog(IReadOnlyList<IRContext> contexts, string s, MessageButtons buttons, CancellationToken ct) {
            await Console.Error.WriteAsync(s);
            while (true) {
                string r = await ReadLineAsync(" [yes/no/cancel]> ", ct);

                if (r.StartsWithIgnoreCase("y")) {
                    return MessageButtons.Yes;
                }
                if (r.StartsWithIgnoreCase("n")) {
                    return MessageButtons.No;
                }
                if (r.StartsWithIgnoreCase("c")) {
                    return MessageButtons.Cancel;
                }

                await Console.Error.WriteAsync("Invalid input, try again!");
            }
        }

        public async Task Plot(PlotMessage plot, CancellationToken ct)
            => await Console.Error.WriteLineAsync(plot.FilePath);

        public async Task WebBrowser(string url, CancellationToken ct)
            => await Console.Error.WriteLineAsync("Browser: " + url);

        public async void DirectoryChanged()
            => await Console.Error.WriteLineAsync("Directory changed.");

        public Task ViewObject(string x, string title, CancellationToken cancellationToken)
            => Console.Error.WriteLineAsync(Invariant($"ViewObjectAsync({title}): {x}"));

        public async Task ViewLibrary(CancellationToken cancellationToken)
            => await Console.Error.WriteLineAsync("ViewLibrary");

        public async Task ShowFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken)
            => await Console.Error.WriteAsync(Invariant($"ShowFile({fileName}, {tabName}, {deleteFile})"));

        public async Task<string> EditFileAsync(string expression, string fileName, CancellationToken cancellationToken) { 
            await Console.Error.WriteAsync(Invariant($"EditFile({expression}, {fileName})"));
            return string.Empty;
        }

        public void PackagesInstalled() => Console.Error.WriteLineAsync("PackagesInstalled").DoNotWait();
        public void PackagesRemoved() => Console.Error.WriteLineAsync("PackagesRemoved").DoNotWait();

        public async Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken) {
            await Console.Error.WriteAsync(Invariant($"fetch_file({remoteFileName}, {localPath})"));
            return localPath;
        }

        private async Task<string> ReadLineAsync(string prompt, CancellationToken ct) {
            while (true) {
                await Console.Out.WriteAsync($"|{_nesting}| {prompt}");
                ++_nesting;
                try {
                    string s = await Console.In.ReadLineAsync();

                    if (s.StartsWithIgnoreCase("$$")) {
                        s = s.Remove(0, 1);
                    } else if (s.StartsWithIgnoreCase("$")) {
                        s = s.Remove(0, 1);

                        var kind = REvaluationKind.Normal;
                        if (s.StartsWithIgnoreCase("!")) {
                            kind |= REvaluationKind.Reentrant;
                            s = s.Remove(0, 1);
                        }

                        var er = await _evaluator.EvaluateAsync(s, kind, ct);
                        await Console.Out.WriteLineAsync(er.ToString());
                        continue;
                    }

                    return s;
                } finally {
                    --_nesting;
                }
            }
        }

        public async Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) {
            await Console.Error.WriteLineAsync(Invariant($"Locator called for {deviceId}."));
            return LocatorResult.CreateNotClicked();
        }

        public async Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) {
            await Console.Error.WriteLineAsync(Invariant($"PlotDeviceCreate called for {deviceId}."));
            return PlotDeviceProperties.Default;
        }

        public async Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) {
            await Console.Error.WriteLineAsync(Invariant($"PlotDeviceDestroy called for {deviceId}."));
        }

        public string GetLocalizedString(string id) => null;
        public Task BeforePackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AfterPackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        class MaxLoggingPermissions : ILoggingPermissions {
            public LogVerbosity CurrentVerbosity { get; set; } = LogVerbosity.Traffic;
            public bool IsFeedbackPermitted => true;
            public LogVerbosity MaxVerbosity => LogVerbosity.Traffic;
        }

        class CoreShell : ICoreShell, IDisposable {
            private readonly IServiceManager _serviceManager;

            public CoreShell(string programName) {
                _serviceManager = new ServiceManager()
                    .AddService(new MaxLoggingPermissions())
                    .AddService(s => new Logger(programName, Path.GetTempPath(), s));
            }

            public IServiceContainer Services => _serviceManager;

            public void Dispose() => _serviceManager.Dispose();
        }
    }
}
