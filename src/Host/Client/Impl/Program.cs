using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    class Program : IRCallbacks {
        private IRExpressionEvaluator _evaluator;
        private int _nesting;

        static void Main(string[] args) {
            var host = new RHost(new Program());
            host.CreateAndRun(args[0]).GetAwaiter().GetResult();
        }

        public void Dispose() {
        }

        public Task Busy(IReadOnlyCollection<IRContext> contexts, bool which, CancellationToken ct) {
            return Task.FromResult(true);
        }

        public Task Evaluate(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct)
        {
            _evaluator = evaluator;
            return Task.CompletedTask;
        }

        public Task Connected(string rVersion) {
            return Task.CompletedTask;
        }

        public Task Disconnected() {
            return Task.CompletedTask;
        }

        public async Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, CancellationToken ct) {
            return (await ReadLineAsync(prompt, ct)) + "\n";
        }

        public async Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct) {
            await Console.Error.WriteLineAsync(s);
        }

        public async Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype, CancellationToken ct) {
            var writer = otype == OutputType.Output ? Console.Out : Console.Error;
            await writer.WriteAsync(buf);
        }

        public async Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct) {
            await Console.Error.WriteAsync(s);
            while (true)
            {
                string r = await ReadLineAsync(" [yes/no/cancel]> ", ct);

                if (r.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Client.YesNoCancel.Yes;
                }
                if (r.StartsWith("n", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Client.YesNoCancel.No;
                }
                if (r.StartsWith("c", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Client.YesNoCancel.Cancel;
                }

                await Console.Error.WriteAsync("Invalid input, try again!");
            }
        }

        public async Task PlotXaml(IReadOnlyCollection<IRContext> contexts, string xamlFilePath, CancellationToken ct) {
            await Console.Error.WriteLineAsync(xamlFilePath);
        }

        private async Task<string> ReadLineAsync(string prompt, CancellationToken ct) {
            while (true) {
                await Console.Out.WriteAsync($"|{_nesting}| {prompt}");
                ++_nesting;
                try {
                    string s = await Console.In.ReadLineAsync();

                    if (s.StartsWith("$$", StringComparison.OrdinalIgnoreCase)) {
                        s = s.Remove(0, 1);
                    } else if (s.StartsWith("$", StringComparison.OrdinalIgnoreCase)) {
                        s = s.Remove(0, 1);
                        bool reentrant = true;
                        if (s.StartsWith("!", StringComparison.OrdinalIgnoreCase)) {
                            reentrant = false;
                        s = s.Remove(0, 1);
                        }
                        var er = await _evaluator.EvaluateAsync(s, reentrant, ct);
                        await Console.Out.WriteLineAsync(er.ToString());
                        continue;
                    }

                    return s;
                } finally {
                    --_nesting;
                }
            }
        }
    }
}
