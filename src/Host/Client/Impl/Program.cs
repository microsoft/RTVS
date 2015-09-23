using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    class Program : IRCallbacks {
        static void Main() {
            var host = new RHost(new Program());
            host.CreateAndRun().GetAwaiter().GetResult();
        }

        public void Dispose() {
        }

        public Task Busy(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, bool which) {
            return Task.FromResult(true);
        }

        public Task Connected(string rVersion) {
            return Task.FromResult(true);
        }

        public Task Disconnected() {
            return Task.FromResult(true);
        }

        public async Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string prompt, string buf, int len, bool addToHistory) {
            return (await ReadLineAsync(prompt, evaluator)) + "\n";
        }

        public async Task ShowMessage(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string s) {
            await Console.Error.WriteLineAsync(s);
        }

        public async Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string buf, OutputType otype) {
            var writer = otype == OutputType.Output ? Console.Out : Console.Error;
            await writer.WriteAsync(buf);
        }

        public async Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string s) {
            await Console.Error.WriteAsync(s);
            while (true)
            {
                string r = await ReadLineAsync(" [yes/no/cancel]> ", evaluator);

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

        private async Task<string> ReadLineAsync(string prompt, IRExpressionEvaluator evaluator) {
            while (true) {
                await Console.Out.WriteAsync(prompt);
                string s = await Console.In.ReadLineAsync();

                if (s.StartsWith("==", StringComparison.OrdinalIgnoreCase)) {
                    s = s.Remove(0, 1);
                } else if (s.StartsWith("=", StringComparison.OrdinalIgnoreCase)) {
                    s = s.Remove(0, 1);
                    var er = await evaluator.EvaluateAsync(s);
                    await Console.Out.WriteLineAsync(er.ToString());
                    continue;
                }

                return s;
            }
        }
    }
}
