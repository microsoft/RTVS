// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.Common.Wpf {
    public static class LambdaProperties {
        public static readonly DependencyProperty ImportedNamespacesProperty = DependencyProperty.RegisterAttached(
            "ImportedNamespaces", typeof(string), typeof(LambdaProperties));

        public static string GetImportedNamespaces(object obj) => null;

        public static void SetImportedNamespaces(object obj, string value) { }
    }
}
