using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    class Program : IRCallbacks {
        private IRExpressionEvaluator _evaluator;

        static void Main(string[] args) {
            var host = new RHost(new Program());
            host.CreateAndRun(args[0]).GetAwaiter().GetResult();
        }

        public void Dispose() {
        }

        public Task Busy(IReadOnlyCollection<IRContext> contexts, bool which) {
            return Task.FromResult(true);
        }

        public Task Evaluate(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator)
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

        public async Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory) {
            return (await ReadLineAsync(prompt)) + "\n";
        }

        public async Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string s) {
            await Console.Error.WriteLineAsync(s);
        }

        public async Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype) {
            var writer = otype == OutputType.Output ? Console.Out : Console.Error;
            await writer.WriteAsync(buf);
        }

        public async Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s) {
            await Console.Error.WriteAsync(s);
            while (true)
            {
                string r = await ReadLineAsync(" [yes/no/cancel]> ");

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

        private async Task<string> ReadLineAsync(string prompt) {
            while (true) {
                await Console.Out.WriteAsync(prompt);
                string s = await Console.In.ReadLineAsync();

                if (s.StartsWith("==", StringComparison.OrdinalIgnoreCase)) {
                    s = s.Remove(0, 1);
                } else if (s.StartsWith("=", StringComparison.OrdinalIgnoreCase)) {
                    s = s.Remove(0, 1);
                    var er = await _evaluator.EvaluateAsync(s);
                    await Console.Out.WriteLineAsync(er.ToString());
                    continue;
                }

                return s;
            }
        }
    }
}
