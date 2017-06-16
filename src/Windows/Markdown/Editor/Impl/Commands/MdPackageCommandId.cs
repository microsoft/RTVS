// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Markdown.Editor.Commands {
    public static class MdPackageCommandId {
        public static readonly Guid MdCmdSetGuid = new Guid("0BF33C69-94C2-4985-81A0-2556F8DB88A6");

        // GuidList.CmdSetGuid
        public const int icmdPreviewHtml = 601;
        public const int icmdPreviewPdf = 602;
        public const int icmdPreviewWord = 603;
        public const int icmdPublish = 610;
        public const int icmdRunRChunk = 611;
        public const int icmdAutomaticSync = 612;
        public const int icmdRunCurrentChunk = 613;
        public const int icmdRunAllChunksAbove = 614;
    }
}
