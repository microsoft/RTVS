// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.R.Wpf.Controls {
    /// <summary>
    /// Interaction logic for IconWithOverlay.xaml
    /// </summary>
    public partial class IconWithOverlay : UserControl {
        public IconWithOverlay() {
            InitializeComponent();
        }

        public object Moniker {
            get => GetValue(MonikerProperty);
            set => SetValue(MonikerProperty, value);
        }

        public object OverlayMoniker {
            get => GetValue(OverlayMonikerProperty);
            set => SetValue(OverlayMonikerProperty, value);
        }

        public Visibility OverlayVisibility {
            get => (Visibility)GetValue(OverlayVisibilityProperty);
            set => SetValue(OverlayVisibilityProperty, value);
        }

        public static readonly DependencyProperty MonikerProperty
            = DependencyProperty.Register("Moniker", typeof(object), typeof(IconWithOverlay), new PropertyMetadata(null));
        public static readonly DependencyProperty OverlayMonikerProperty
            = DependencyProperty.Register("OverlayMoniker", typeof(object), typeof(IconWithOverlay), new PropertyMetadata(null));
        public static readonly DependencyProperty OverlayVisibilityProperty
            = DependencyProperty.Register("OverlayVisibility", typeof(Visibility), typeof(IconWithOverlay), new PropertyMetadata(Visibility.Collapsed));

        protected override void OnInitialized(EventArgs e) {
            OverlayImage.Width = 0.55 * MainImage.Width;
            OverlayImage.Height = 0.55 * MainImage.Height;
            base.OnInitialized(e);
        }
    }
}
