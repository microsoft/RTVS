// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Editor.Projection {
    public sealed class MappingsChangedEventArgs : EventArgs {
        public IReadOnlyList<ProjectionMapping> Mappings { get; }
        public string Text { get; }

        public MappingsChangedEventArgs(string text, IReadOnlyList<ProjectionMapping> mappings) {
            Text = text;
            Mappings = mappings;
        }
    }
}
