// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Markdown.Editor.Preview {
    public interface IMarkdownPreview {
        void Update();
        Task RunCurrentChunkAsync();
        Task RunAllChunksAboveAsync();
    }
}
