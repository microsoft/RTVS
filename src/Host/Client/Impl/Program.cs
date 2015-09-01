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

        public Task Busy(IReadOnlyCollection<IRContext> contexts, bool which) {
            return Task.FromResult(true);
        }

        public Task Connected(string rVersion) {
            return Task.FromResult(true);
        }

        public Task Disconnected() {
            return Task.FromResult(true);
        }

        public async Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory) {
            await Console.Out.WriteAsync(prompt);
            return (await Console.In.ReadLineAsync()) + "\n";
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
                await Console.Error.WriteAsync(" [yes/no/cancel]> ");
                string r = await Console.In.ReadLineAsync();

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
    }
}
