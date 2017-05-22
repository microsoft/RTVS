// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Protocol {
    public class VideoCardInfo {
        public string VideoCardName { get; set; }
        public long VideoRAM { get; set; }
        public string VideoProcessor { get; set; }
    }
}
