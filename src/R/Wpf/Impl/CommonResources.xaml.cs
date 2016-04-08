// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows;

namespace Microsoft.R.Wpf {
    public partial class CommonResources : ResourceDictionary {
        public static Collection<ResourceDictionary> Instance { get; } = new Collection<ResourceDictionary> {
            new CommonResources()
        };

        public CommonResources() {
            InitializeComponent();
        }
    }
}
