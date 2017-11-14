// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.Common.Wpf {
    public static class AttachedProperties {
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.RegisterAttached("IsValid", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));

        public static bool GetIsValid(FrameworkElement fe) => (bool)(fe.GetValue(IsValidProperty) ?? false);
        public static void SetIsValid(FrameworkElement fe, bool value) => fe.SetValue(IsValidProperty, value);
    }
}
