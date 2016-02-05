using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger.Test {
    sealed class SourceFile : IDisposable {
        public string FilePath { get; }

        public SourceFile(string content) {
            FilePath = Path.GetTempFileName();
            using (var sw = new StreamWriter(FilePath)) {
                sw.Write(content);
            }
        }

        public async Task Source(IRSession session) {
            using (IRSessionInteraction eval = await session.BeginInteractionAsync()) {
                await eval.RespondAsync($"rtvs::debug_source({FilePath.ToRStringLiteral()})" + Environment.NewLine);
            }
        }

        public void Dispose() {
            try {
                File.Delete(FilePath);
            } catch (IOException) { }
        }
    }
}
