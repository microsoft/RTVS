// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Outline {
    public sealed class OutlineRegionsChangedEventArgs : EventArgs {
        public OutlineRegionCollection Regions { get; private set; }
        public ITextRange ChangedRange { get; private set; }

        public OutlineRegionsChangedEventArgs(OutlineRegionCollection regions, ITextRange changedRange) {
            Regions = regions;
            ChangedRange = changedRange;
        }
    }
}
