// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Test {
    public sealed class SourceFile : IDisposable {
        public string FilePath { get; }

        public SourceFile(string content) {
            FilePath = Path.GetTempFileName();
            using (var sw = new StreamWriter(FilePath)) {
                sw.Write(content);
            }
        }

        public async Task Source(IRSession session, bool debug = true) {
            using (IRSessionInteraction eval = await session.BeginInteractionAsync()) {
                await eval.RespondAsync($"{(debug ? "rtvs::debug_source" : "source")}({FilePath.ToRStringLiteral()})" + Environment.NewLine);
            }
        }

        public void Dispose() {
            try {
                File.Delete(FilePath);
            } catch (IOException) { }
        }
    }
}
