// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline {
    internal sealed class RSectionsCollection {
        private readonly List<ITrackingSpan> _spans = new List<ITrackingSpan>();
    }
}
