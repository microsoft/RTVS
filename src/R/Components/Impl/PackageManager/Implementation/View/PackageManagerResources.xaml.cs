// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    internal partial class PackageManagerResources : ResourceDictionary {
        public static Collection<ResourceDictionary> Instance { get; } = new Collection<ResourceDictionary> {
            new PackageManagerResources()
        };

        public PackageManagerResources() {
            InitializeComponent();
        }

        private void PackageIconImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
            var image = sender as Image;
            if (image != null) {
                image.Source = Images.DefaultPackageIcon;
            }
        }
    }
}
