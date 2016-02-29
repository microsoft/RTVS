// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal sealed class LocDefaultValueAttribute : DefaultValueAttribute {
        public LocDefaultValueAttribute(string resourceId) : base(string.Empty) {
            SetValue(Resources.ResourceManager.GetString(resourceId));
        }
     }
}
